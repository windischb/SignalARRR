using doob.SignalARRR.Common.RemoteReferenceTypes;

namespace doob.SignalARRR.Client;

public class StreamReferenceResolver(StreamReference streamReference) {


    public async Task<Stream> ProcessStreamArgument() {

        var uri = new Uri(streamReference.Uri);
        switch (uri.Scheme.ToLower()) {

            case "http":
            case "https": {
                return await DownloadStream(uri);
                    
            }
            default: {
                throw new Exception($"StreamReference.Scheme '{uri.Scheme}' is not implemented!");
            }
        }
    }


    private static async Task<Stream> DownloadStream(Uri uri) {
        var httpClient = new HttpClient();
        var res = await httpClient.GetAsync(uri);
        return await res.Content.ReadAsStreamAsync();

    }

}
