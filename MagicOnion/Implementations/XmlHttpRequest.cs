using MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Avae.DAL;

[SupportedOSPlatform("browser")]
public partial class XmlHttpRequest : IXmlHttpRequest
{
    [JSImport("globalThis.eval")]
    public static partial string Fetch(string request);

    public byte[] Send(string url, string parameters, byte[] data, int timeout)
    {
        string? fullUrl = string.Concat(url, parameters)
            .Replace("\\", "\\\\")
            .Replace("'", "\\'");

        string? rawResponse = null;
        var task = new Task(() =>
        {
            rawResponse = Fetch($@"
(function() {{
    var url = '{fullUrl}';
    var payloadB64 = '{Convert.ToBase64String(data)}';

    // Decode base64 → Uint8Array
    var binaryString = atob(payloadB64);
    var payload = new Uint8Array(binaryString.length);
    for (var i = 0; i < binaryString.length; i++) {{
        payload[i] = binaryString.charCodeAt(i);
    }}

    // Build 5-byte frame
    var length = payload.length;
    var frame = new Uint8Array(5 + length);
    frame[0] = 0x00;
    frame[1] = (length >>> 24) & 0xff;
    frame[2] = (length >>> 16) & 0xff;
    frame[3] = (length >>>  8) & 0xff;
    frame[4] =  length        & 0xff;
    frame.set(payload, 5);

    // Encode the whole frame as base64 (required for text mode)
    var frameB64 = btoa(String.fromCharCode.apply(null, frame));

    var xhr = new XMLHttpRequest();    
    xhr.open('POST', url, false);                 // still sync

    xhr.setRequestHeader('Content-Type', 'application/grpc-web-text');
    xhr.setRequestHeader('Accept', 'application/grpc-web-text');
    xhr.setRequestHeader('X-Grpc-Web', '1');
    xhr.setRequestHeader('grpc-timeout', '1S');
    xhr.send(frameB64);                           // send base64 string

    if (xhr.status !== 200) {{
        return '';
    }}

    // Get raw response
    return xhr.responseText || '';

}})();
");
        });
        task.RunSynchronously();
        if (string.IsNullOrEmpty(rawResponse))
            return Array.Empty<byte>();

        var response = ParseGrpcResponse(rawResponse);
        var result = MessagePackSerializer.Deserialize<DBResult>(response);
        if (true == result?.Successful)
            return result.Data ?? Array.Empty<byte>();

        Debug.WriteLine(result?.Exception);
        return Array.Empty<byte>();
    }


    private byte[] ParseGrpcResponse(string rawResponse)
    {
        // The trailer is: gAAAABBncnBjLXN0YXR1czogMA0K
        // Which decodes to: grpc-status: 0\r\n

        // Method 1: Split by the trailer pattern
        string? trailerPattern = "gAAAABBncnBjLXN0YXR1czogMA0K";
        string? base64Part = rawResponse;

        int trailerIndex = rawResponse.IndexOf(trailerPattern);
        if (trailerIndex > 0)
        {
            base64Part = rawResponse.Substring(0, trailerIndex);
        }
        else
        {
            // Method 2: Use regex to extract base64
            var match = System.Text.RegularExpressions.Regex.Match(rawResponse, @"^([A-Za-z0-9+/=]+)");
            if (match.Success)
            {
                base64Part = match.Groups[1].Value;
            }
        }

        // Clean and fix padding
        base64Part = System.Text.RegularExpressions.Regex.Replace(base64Part, @"[^A-Za-z0-9+/=]", "");
        int mod = base64Part.Length % 4;
        if (mod > 0)
        {
            base64Part = base64Part.PadRight(base64Part.Length + (4 - mod), '=');
        }

        // Decode base64
        byte[] responseBytes = Convert.FromBase64String(base64Part);

        // Parse gRPC frames
        return ParseGrpcFrames(responseBytes);
    }


    private byte[] ParseGrpcFrames(byte[] responseBytes)
    {
        var result = new List<byte>();
        int offset = 0;

        while (offset + 5 <= responseBytes.Length)
        {
            byte? flags = responseBytes[offset];
            int msgLen = (responseBytes[offset + 1] << 24) |
                         (responseBytes[offset + 2] << 16) |
                         (responseBytes[offset + 3] << 8) |
                          responseBytes[offset + 4];
            offset += 5;

            if (offset + msgLen > responseBytes.Length)
                break;

            byte[] frameData = new byte[msgLen];
            Array.Copy(responseBytes, offset, frameData, 0, msgLen);
            offset += msgLen;

            if ((flags & 0x80) == 0x80)
            {
                // Trailer frame - check status
                string? trailerText = Encoding.UTF8.GetString(frameData);
                if (trailerText.Contains("grpc-status: 0"))
                {
                    continue; // Success
                }
                else
                {
                    // Error
                    return Array.Empty<byte>();
                }
            }
            else
            {
                // Data frame
                result.AddRange(frameData);
            }
        }

        return result.ToArray();
    }
}
