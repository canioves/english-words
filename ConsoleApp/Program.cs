using System.Net.Http.Json;
using System.Text.Json;
using System.Xml;

App app = new App();

bool keepAlive = true;
while (keepAlive)
{
    RandomWord randomWord = app.GetRandomWord().Result;

    Console.WriteLine($"Переводы слова {randomWord.Word}:");
    List<string> results = app.GetData(randomWord.Word).Result;
    foreach (string item in results)
    {
        Console.WriteLine(item);
    }

    ConsoleKey key = Console.ReadKey().Key;
    keepAlive = key != ConsoleKey.Escape;
    Console.Clear();
}

public class App
{
    private ApiKeys Keys { get; }
    public App()
    {
        using StreamReader sr = new StreamReader("supersecret.json");
        string json = sr.ReadToEnd();
        Keys = JsonSerializer.Deserialize<ApiKeys>(json);
    }
    public HttpClient http = new HttpClient();
    public async Task<List<string>> GetData(string word)
    {
        string key = Keys.DictKey;
        string baseUrl = "https://dictionary.yandex.net/api/v1/dicservice/lookup";
        UriBuilder builder = new UriBuilder(baseUrl);
        builder.Query = $"key={key}&lang=en-ru&text={word}";
        string url = builder.ToString();

        using var result = await http.GetAsync(url);
        string text = await result.Content.ReadAsStringAsync();

        return GetTranslations(text);
    }

    public async Task<RandomWord> GetRandomWord()
    {
        string key = Keys.Wordkey;
        string baseUrl = "https://api.api-ninjas.com/v1/randomword";

        using var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
        request.Headers.Add("X-Api-Key", key);

        using var result = await http.SendAsync(request);
        RandomWord word = await result.Content.ReadFromJsonAsync<RandomWord>();

        return word;
    }

    private List<string> GetTranslations(string text)
    {
        var xml = new XmlDocument();
        xml.LoadXml(text);

        List<string> trList = new List<string>();
        XmlNodeList root = xml.GetElementsByTagName("def");
        foreach (XmlNode defNode in root)
        {
            XmlNodeList defNodes = defNode.ChildNodes;
            foreach (XmlNode defChildNode in defNodes)
            {
                if (defChildNode.Name == "text") continue;
                XmlNodeList trNodes = defChildNode.ChildNodes;
                foreach (XmlNode trChildNode in trNodes)
                {
                    if (trChildNode.Name == "mean") continue;
                    trList.Add(trChildNode.InnerText);
                }
            }
        }
        return trList;
    }
}

public class ApiKeys
{
    public string DictKey { get; set; }
    public string Wordkey { get; set; }
}

public class RandomWord
{
    public string Word { get; set; }
}