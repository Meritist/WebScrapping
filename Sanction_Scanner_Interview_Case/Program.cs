using HtmlAgilityPack;
using Sanction_Scanner_Interview_Case.Models;

const string CrawlUrl = @"https://www.arabam.com/";

static List<VehicleModel> Crawl()
{
    //HTMLWeb objemizi oluşturup cookielere izin veriyoruz
    HtmlWeb Web = new HtmlWeb() { UseCookies = true };
    Web.PreRequest += request =>
    {
        // gets access to the cookie container
        var cookieContainer = request.CookieContainer;
        //  gets access to the request headers
        var headers = request.Headers;
        return true;
    };
    Web.PostResponse += (request, response) =>
    {
        // response headers
        var headers = response.Headers;
        // cookies
        var cookies = response.Cookies;
    };
    List<VehicleModel> VehicleList = new();

    //Ana Sayfanızın Htmlini HtmlDocument' e yüklüyoruz
    var HtmlDocument = Web.Load(CrawlUrl);
    //Sayfanın içinden Vitrin sectionunu çekiyoruz
    HtmlNode MainShowCase = HtmlDocument.DocumentNode.SelectNodes("//div[@class='row']")[0];
    int Counter = 0;
    //Vitrin sectionundaki her bir ilandaki isim, fiyat ve linklerini kaydediyoruz
    foreach (HtmlNode VehicleNode in MainShowCase.SelectNodes("div[@class='col-lg-2 col-md-3 col-xs-6']"))
    {
        if (Counter == 30)
        {
            //Vitrindeki 30uncu ilandan sonra bazen reklam gelebildiğini farkettim engellemek amacıyla bu kontrolü koydum
            break;
        }
        VehicleModel Vehicle = new();
        //İlanın linkini kaydediyoruz
        HtmlNode VehicleAElement = VehicleNode.SelectNodes("//div[@class='content-container']/a[@href]")[Counter];
        Vehicle.Link = VehicleAElement.GetAttributeValue("href", string.Empty);
        //İlanın adını kaydediyoruz
        Vehicle.Name = VehicleNode.SelectNodes("//h4[@class='model-name']")[Counter].InnerHtml.Trim();
        //İlanın fiyatını kaydediyoruz
        Vehicle.Price = VehicleNode.SelectNodes("//div[@class='price']")[Counter].InnerHtml.Trim();
        VehicleList.Add(Vehicle);
        Counter++;
    }
    return (VehicleList);
}

static List<VehicleModel> ScrapingDetails(List<VehicleModel> VehicleList)
{
    List<VehicleModel> FinalVehicleList = new();
    HtmlWeb Web = new HtmlWeb() { UseCookies = true };
    Web.PreRequest += request =>
    {
        // gets access to the cookie container
        var cookieContainer = request.CookieContainer;
        //  gets access to the request headers
        var headers = request.Headers;
        return true;
    };
    Web.PostResponse += (request, response) =>
    {
        // response headers
        var headers = response.Headers;
        // cookies
        var cookies = response.Cookies;
    };
    foreach (VehicleModel Vehicle in VehicleList)
    {
        try
        {
            Vehicle.Properties = new Dictionary<string, string>();
            //Aracın detayının bulunduğu sayfaya gidiyoruz
            var HTMLDocument = Web.Load(CrawlUrl + Vehicle.Link);
            var VehicleMenu = HTMLDocument.DocumentNode.SelectSingleNode("//ul[@class='w100 cf mt12 detail-menu']");
            HtmlNodeCollection VehicleItems = VehicleMenu.SelectNodes("//li[@class='bcd-list-item']");
            //Aracın özelliklerini tek tek bir dictionarye kaydediyoruz
            //Araba, karavan, motorsiklet gibi farklı araçlar olduğundan her aracın farklı özellikleri olabiliyor
            //Bu yüzden Dictionary de tutmayı tercih ettim.
            foreach (HtmlNode VehicleAttribute in VehicleItems)
            {
                Vehicle.Properties.Add(VehicleAttribute.ChildNodes[1].InnerHtml.Trim(), VehicleAttribute.ChildNodes[3].InnerHtml.Trim());
            }
            FinalVehicleList.Add(Vehicle);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    return (FinalVehicleList);
}
//Anasayfadaki tüm ilanları okuyoruz
List<VehicleModel> result = Crawl();
//Okuduğumuz her bir ilanın sayfasına giderek detaylarını okuyoruz
result = ScrapingDetails(result);
List<decimal> AveragePrice = new();
foreach (VehicleModel Vehicle in result)
{
    //Tek tek araçların isim ve fiyatlarını ekrana yazıyoruz
    Console.WriteLine(Vehicle.Name + " - " + Vehicle.Price);
    //Daha sonra fiyat ortalaması gösterebilmek için fiyatları decimal olacak şekilde topluyoruz
    AveragePrice.Add(Convert.ToDecimal(Vehicle.Price.Substring(0, Vehicle.Price.IndexOf(" "))));
}
Console.WriteLine("Tüm araç fiyatlarının ortlaması : " + AveragePrice.Average());
using (StreamWriter writer = new StreamWriter("arabam_com_vehicles_and_prices.txt"))
{
    foreach (VehicleModel Vehicle in result)
    {
        //Bu araçları txt dosyasına kaydediyoruz
        writer.WriteLine(Vehicle.Name + " - " + Vehicle.Price);
    }
}
