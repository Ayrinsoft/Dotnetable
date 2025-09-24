using Dotnetable.Shared.DTO.Public;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dotnetable.Shared.Tools;
public static class GeneralEvents
{
    public static string ToJsonString(this object inputObject) => JsonSerializer.Serialize(inputObject);

    public static string ToJsonStringUnSafe(this object inputObject) => JsonSerializer.Serialize(inputObject, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

    public static T JsonToObject<T>(this string jsonObject) where T : new()
    {
        if (string.IsNullOrEmpty(jsonObject) || string.IsNullOrWhiteSpace(jsonObject) || jsonObject == "\"") return new T();
        jsonObject = jsonObject.Replace("\r", "").Replace("\n", "");

        try
        {
            return JsonSerializer.Deserialize<T>(jsonObject, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception)
        {
            return new T();
        }
    }

    public static T JsonToObjectUnSafe<T>(this string jsonObject) where T : new()
    {
        if (string.IsNullOrEmpty(jsonObject) || string.IsNullOrWhiteSpace(jsonObject) || jsonObject == "\"") return new T();
        jsonObject = jsonObject.Replace("\r", "").Replace("\n", "");

        try
        {
            return JsonSerializer.Deserialize<T>(jsonObject, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, PropertyNameCaseInsensitive = true });
        }
        catch (Exception)
        {
            return new T();
        }

    }

    public static T CastModel<T>(this object inputObject) where T : new()
    {
        try
        {
            return JsonToObject<T>(inputObject.ToJsonString());
        }
        catch (Exception)
        {
            return new T();
        }
    }

    public static string FileNameCorrection(this string fileName, int maxLength = 36)
    {
        if (fileName.Length < 4)
            fileName = $"{new Guid().ToString()[..24]}.{fileName.Split('.').LastOrDefault()}";

        if (fileName.Length > maxLength)
            fileName = $"{fileName[..(maxLength - 5)]}.{fileName.Split('.').LastOrDefault()}";

        return fileName;
    }

    private static readonly string[] _sqlCheckList = { "--", ";--", ";", "/*", "*", "#", "-", "*/", "@@", "@", "char", "nchar", "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end", "exec", "execute", "fetch", "insert", "kill", "select", "sys", "sysobjects", "syscolumns", "table", "update", "dec", "proc" };
    public static string CheckForInjection(this string query, List<string> orderKeys = null)
    {
        if (string.IsNullOrWhiteSpace(query)) return "";
        List<string> orderItems = query.Split(',').Where(i => !i.Contains(';') && !i.Contains('-') && !i.Contains('*')).ToList();

        if ((from i in orderItems where _sqlCheckList.Any(j => j.Equals(i.Split(' ')[0], StringComparison.OrdinalIgnoreCase)) select i).Any())
            return "";

        if (!string.IsNullOrWhiteSpace(query) && orderKeys != null && orderKeys.Count > 0)
        {
            if (orderItems.Count > 0)
            {
                var drivedList = (from i in orderKeys where i.Contains('|') select i).ToList();
                var normalList = (from i in orderKeys where !i.Contains('|') select i).ToList();

                var orderString = new StringBuilder();
                foreach (var j in orderItems)
                {
                    bool descendingItem = j.Contains(" DESC", StringComparison.OrdinalIgnoreCase);
                    string queryItem = j.Replace("DESC", "", StringComparison.OrdinalIgnoreCase).Trim();

                    var checkParameter = (from i in normalList where i.Equals(queryItem, StringComparison.OrdinalIgnoreCase) select i).FirstOrDefault();
                    if (checkParameter != null && checkParameter.Length > 0)
                    {
                        if (orderString.Length > 0) orderString.Append(", ");
                        orderString.Append($"{checkParameter}{(descendingItem ? " DESC" : "")}");
                        continue;
                    }

                    var checkDrived = (from i in drivedList where i.Split('|')[0].Equals(queryItem, StringComparison.OrdinalIgnoreCase) select i).FirstOrDefault();
                    if (checkDrived != null && checkDrived.Length > 0)
                    {
                        if (orderString.Length > 0) orderString.Append(", ");
                        orderString.Append($"{checkDrived.Split('|')[1]}{(descendingItem ? " DESC" : "")}");
                    }
                }
                query = orderString.ToString();
            }
        }

        return query;
    }

    public static string GenerateRandomCode(int codeLength)
    {
        Random r = new();
        StringBuilder s = new();

        for (int j = 0; j < codeLength; j++)
        {
            int i = r.Next(3);
            var ch = i switch
            {
                1 => r.Next(48, 57),
                2 => r.Next(65, 90),
                3 => r.Next(97, 122),
                _ => r.Next(65, 90)
            };
            s.Append(Convert.ToChar(ch));
            r.NextDouble();
            r.Next(100, 19999);
        }
        var ret = s.ToString();
        ret = ret.ToUpper().Replace("0", "1").Replace("O", "F");
        if (ret.Trim().Length != codeLength) return GenerateRandomCode(codeLength);
        return ret;
    }

    public static string GenerateRandomPassword(int passwordLength)
    {
        Random r = new();
        StringBuilder s = new();
        for (int j = 0; j < passwordLength; j++)
        {
            int i = r.Next(4);
            var ch = i switch
            {
                1 => r.Next(33, 43),
                2 => r.Next(48, 57),
                3 => r.Next(60, 90),
                4 => r.Next(97, 122),
                _ => r.Next(97, 122)
            };
            s.Append(Convert.ToChar(ch));
            r.NextDouble();
            r.Next(100, 19999);
        }
        return s.ToString();
    }

    public static async Task<HttpClientServiceCallResponse> HttpClientReceive(HttpMethod method, string urlAddress, string requestBody = "", int timeOutInSecound = 400, Dictionary<string, string> headerParams = null, RequestContentType contentTypeRequest = RequestContentType.Json)
    {
        HttpClientServiceCallResponse finallResponse = null;
        try
        {
            var requestor = new HttpRequestMessage(method, urlAddress) { Content = new StringContent(requestBody) };

            if (contentTypeRequest != RequestContentType.None)
                requestor.Content.Headers.ContentType = new MediaTypeHeaderValue(contentTypeRequest switch { RequestContentType.Json => "application/json", RequestContentType.XML => "application/xml", RequestContentType.UrlEncode => "application/x-www-form-urlencoded", _ => "application/json" });

            using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli }) { Timeout = TimeSpan.FromSeconds(timeOutInSecound) };

            if (headerParams is not null && headerParams.Count > 0)
            {
                foreach (var j in headerParams)
                {
                    if (j.Key != "Content-Type")
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(j.Key, j.Value);
                    }
                    else
                    {
                        if (j.Value.Contains(';'))
                            requestor.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(j.Value);
                        else
                            requestor.Content.Headers.ContentType = new MediaTypeHeaderValue(j.Value);
                    }
                }
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeOutInSecound));
            cts.Token.ThrowIfCancellationRequested();

            var apiResponse = await client.SendAsync(requestor);

            if (apiResponse.StatusCode == HttpStatusCode.OK)
            {
                finallResponse = new()
                {
                    IsSuccess = true,
                    ResponseBody = await apiResponse.Content.ReadAsStringAsync().ConfigureAwait(false)
                };
            }
            else
            {
                finallResponse = new()
                {
                    IsSuccess = false,
                    ResponseBody = $"StatusCode: {apiResponse.StatusCode}"
                };
            }
        }
        catch (Exception x) { finallResponse = new() { IsSuccess = false, ResponseBody = x.Message, ErrorException = new() { ErrorCode = "SX", Message = x.Message } }; }

        return finallResponse;
    }


    public static bool ValidateEmail(this string emailAddress) => MailAddress.TryCreate(emailAddress, out var _);


    public static string ClearURLString(this string urlAddress)
    {
        urlAddress = Regex.Replace(urlAddress, @"&quot;|['"",&?%\.!()@$^_+=*:#/\\-]", " ").Trim();
        urlAddress = Regex.Replace(urlAddress, @"\s+", "-");
        urlAddress = Regex.Replace(urlAddress, @"\-{2,}", "-");
        if (urlAddress.Length > 80) urlAddress = urlAddress[..80];
        return urlAddress.TrimEnd(new[] { '-' });
    }

}
