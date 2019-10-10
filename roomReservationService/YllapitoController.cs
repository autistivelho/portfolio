using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RavintolaTalliYllapito.Models;

namespace RavintolaTalliYllapito.Controllers
{
    public class YllapitoController : Controller
    {
        public List<Asiakas> Asiakkaat = new Yllapitomalli().HaeKaikkiAsiakkaat();
        public List<Tila> Tilat = new Yllapitomalli().HaeTilat();

        public ActionResult Start()
        {
            return RedirectToAction("Index");
        }

        public ActionResult Index(string luonti, string etunimi, string sukunimi, string sahkoposti,
            string puhelinnumero, string katuosoite, string postinumero, string paikkakunta, string tilaId,
            string henkilomaara, string narikka, string kahvitus, string vahtimestari, string tekninen,
            string alkamisPvm, string alkamisKellonaika, string loppumisPvm, string loppumiskellonaika,
            string maksutapa)
        {
            var yllapitomalli = new Yllapitomalli();
            
            if (luonti != null)
            {
                try
                {
                    var asiakas = new Asiakas(
                        etunimi,
                        sukunimi,
                        sahkoposti,
                        puhelinnumero,
                        katuosoite,
                        int.Parse(postinumero),
                        paikkakunta);

                    var valitutPalvelut = "";

                    if (narikka != null)
                    {
                        valitutPalvelut += "narikka";
                    }

                    if (kahvitus != null)
                    {
                        valitutPalvelut += valitutPalvelut != "" ? ", kahvitus" : "kahvitus";
                    }

                    if (vahtimestari != null)
                    {
                        valitutPalvelut += valitutPalvelut != "" ? ", vahtimestari" : "vahtimestari";
                    }

                    if (tekninen != null)
                    {
                        valitutPalvelut += valitutPalvelut != "" ? ", tekninen tuki" : "tekninen tuki";
                    }

                    var alkamisAika =
                        ((DateTimeOffset) new DateTime(DateTime.ParseExact($"{alkamisPvm} {alkamisKellonaika}",
                            "dd.MM.yyyy HH.mm", CultureInfo.InvariantCulture).Ticks)).ToUnixTimeSeconds();
                    var loppumisAika =
                        ((DateTimeOffset) new DateTime(DateTime.ParseExact($"{loppumisPvm} {loppumiskellonaika}",
                            "dd.MM.yyyy HH.mm", CultureInfo.InvariantCulture).Ticks)).ToUnixTimeSeconds();

                    Asiakkaat = yllapitomalli.LisaaVaraus(asiakas, Int32.Parse(tilaId), Int32.Parse(henkilomaara),
                        valitutPalvelut, alkamisAika, loppumisAika, maksutapa, 200, Asiakkaat);

                    var varaukset = yllapitomalli.HaeKaikkiVaraukset();

                    var model = new List<object> {varaukset, Tilat};

                    return View(model);
                }
                catch (Exception e)
                {
                    ViewBag.Virheilmoitus = "Virhe varauksen lisäämisessä." + e;
                    Debug.WriteLine("kätsi");

                    var varaukset = yllapitomalli.HaeKaikkiVaraukset();

                    var model = new List<object> {varaukset, Tilat};

                    return View(model);
                }

            }
            else
            {
                var varaukset = yllapitomalli.HaeKaikkiVaraukset();

                var model = new List<object> {varaukset, Tilat};

                return View(model);
            }
        }

        //POST: /yllapito/delete/id
        [HttpPost]
        public ActionResult Delete(string id)
        {
            var yllapitomalli = new Yllapitomalli();
            yllapitomalli.PoistaVaraus(int.Parse(id));

            return RedirectToAction("Index");
        }
    }
}