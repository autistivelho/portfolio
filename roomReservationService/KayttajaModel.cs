using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace RavintolaTalliVarauspalvelu.Models
{
    public class KayttajaModel : Tietokantayhteys
    {
        public List<Tila> HaeTilat(int varausId = -1)
        {
            try
            {
                string sqlLause;
                MySqlCommand komento;

                if (varausId != -1)
                {
                    sqlLause = "SELECT tilat.* FROM tilarivit JOIN tilat ON tilarivit.tilaid = tilat.id WHERE varausid = @varausId;";

                    komento = new MySqlCommand(sqlLause, Yhteys);
                    komento.Parameters.Add("@varausId", MySqlDbType.Int32);
                    komento.Parameters["@varausId"].Value = varausId;
                }
                else
                {
                    sqlLause = "SELECT * FROM tilat;";
                    komento = new MySqlCommand(sqlLause, Yhteys);
                }

                var tulokset = komento.ExecuteReader();

                var tilat = new List<Tila>();

                while (tulokset.Read())
                {
                    tilat.Add(new Tila(tulokset.GetInt32("id"),
                        tulokset.GetString("nimi"),
                        tulokset.GetString("katuosoite"),
                        tulokset.GetString("paikkakunta"),
                        tulokset.GetString("rakennus"),
                        tulokset.GetString("varustus"),
                        tulokset.GetInt32("kapasiteetti"),
                        tulokset.GetInt32("hinta")));
                }

                return tilat;
            }
            catch (Exception e)
            {
                throw new Exception("Virhe tilojen hakemisessa. ", e);
            }
        }

        ////////

        public List<Lisapalvelu> HaeLisapalvelut(int varausId = -1)
        {
            try
            {
                string sqlLause;
                MySqlCommand komento;

                if (varausId != -1)
                {
                    sqlLause = "SELECT lisapalvelut.*, lisapalvelurivit.maara FROM lisapalvelurivit JOIN lisapalvelut ON lisapalvelurivit.lisapalveluid = lisapalvelut.id WHERE varausid = @varausId;";
                    komento = new MySqlCommand(sqlLause, Yhteys);
                    komento.Parameters.Add("@varausId", MySqlDbType.Int32);
                    komento.Parameters["@varausId"].Value = varausId;
                }
                else
                {
                    sqlLause = "SELECT * FROM lisapalvelut;";
                    komento = new MySqlCommand(sqlLause, Yhteys);
                }

                var tulokset = komento.ExecuteReader();

                var lisapalvelut = new List<Lisapalvelu>();

                while (tulokset.Read())
                {
                    lisapalvelut.Add(new Lisapalvelu(tulokset.GetInt32("id"),
                        tulokset.GetString("nimi"),
                        tulokset.GetInt32("maara")));
                }

                return lisapalvelut;
            }
            catch (Exception e)
            {
                throw new Exception("Virhe palveluiden hakemisessa. ", e);
            }
        }

        public bool LisaaVaraus(Asiakas asiakas, Tila tila, int henkilomaara, string lisapalvelut, long aloitusPvm, long lopetusPvm, string maksutapa, int summa, List<Asiakas> asiakkaat, List<Tila> tilat)
        {
            try
            {
                if (!asiakkaat.Contains(asiakas))
                {
                    asiakkaat = LisaaAsiakas(asiakas, asiakkaat);
                }

                var asiakasId = asiakkaat.FindIndex(x => x == asiakas) + 1;
                var tilaId = tilat.FindIndex(x => x == tila) + 1;

                const string sqlLause = "INSERT INTO varaukset (asiakasID, tilaID, aloituspvm, lopetuspvm, maksutapa, summa, henkilomaara, lisapalvelut) VALUES (@asiakasID, @tilaID, @aloituspvm, @lopetuspvm, @maksutapa, @summa, @henkilomaara, @lisapalvelut);";

                var komento = new MySqlCommand(sqlLause, Yhteys);
                komento.Parameters.Add("@asiakasID", MySqlDbType.Int32).Value = asiakasId;
                komento.Parameters.Add("@tilaID", MySqlDbType.Int32).Value = tilaId;
                komento.Parameters.Add("@aloituspvm", MySqlDbType.Int64).Value = aloitusPvm;
                komento.Parameters.Add("@lopetuspvm", MySqlDbType.Int64).Value = lopetusPvm;
                komento.Parameters.Add("@maksutapa", MySqlDbType.String).Value = maksutapa;
                komento.Parameters.Add("@summa", MySqlDbType.Int32).Value = summa;
                komento.Parameters.Add("@henkilomaara", MySqlDbType.Int32).Value = henkilomaara;
                komento.Parameters.Add("@lisapalvelut", MySqlDbType.String).Value = lisapalvelut;

                komento.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception("Virhe varauksen lisäämisessä. ", e);
            }
        }

        /// <summary>
        /// Lisää asiakkaan tietokantaan. Palauttaa boolean-arvona true, jos lisäys onnistui.
        /// </summary>
        /// <param name="asiakas">Asiakkaan tiedot Asiakas-tietomuodossa.</param>
        /// <returns></returns>
        public List<Asiakas> LisaaAsiakas(Asiakas asiakas, List<Asiakas> asiakkaat)
        {
            try
            {
                const string sqlLause = "INSERT INTO asiakkaat (etunimi, sukunimi, sahkoposti, puh, katuosoite, postinumero, paikkakunta) VALUES(@etunimi , @sukunimi, @sahkoposti, @puhelinnumero, @katuosoite, @postinumero, @paikkakunta);";

                var komento = new MySqlCommand(sqlLause, Yhteys);
                komento.Parameters.Add("@etunimi", MySqlDbType.String).Value = asiakas.Etunimi;
                komento.Parameters.Add("@sukunimi", MySqlDbType.String).Value = asiakas.Sukunimi;
                komento.Parameters.Add("@sahkoposti", MySqlDbType.String).Value = asiakas.Sahkoposti;
                komento.Parameters.Add("@puhelinnumero", MySqlDbType.Int32).Value = asiakas.Puhelinnumero;
                komento.Parameters.Add("@katuosoite", MySqlDbType.String).Value = asiakas.Katuosoite;
                komento.Parameters.Add("@postinumero", MySqlDbType.Int32).Value = asiakas.Postinumero;
                komento.Parameters.Add("@paikkakunta", MySqlDbType.String).Value = asiakas.Paikkakunta;

                komento.ExecuteNonQuery();

                asiakkaat.Add(asiakas);

                return asiakkaat;
            }
            catch (Exception e)
            {
                throw new Exception("Virhe uuden asiakkaan lisäämisessä. ", e);
            }
        }
        
        public List<Asiakas> HaeKaikkiAsiakkaat()
        {
            try
            {
                const string sqlLause = "SELECT * FROM asiakkaat;";

                var tulokset = new MySqlCommand(sqlLause, Yhteys).ExecuteReader();

                var asiakkaat = new List<Asiakas>();

                while (tulokset.Read())
                {
                    asiakkaat.Add(new Asiakas(tulokset.GetInt16("id"),tulokset.GetString("etunimi"),
                        tulokset.GetString("sukunimi"),
                        tulokset.GetString("sahkoposti"),
                        tulokset.GetString("puh"),
                        tulokset.GetString("katuosoite"),
                        tulokset.GetInt32("postinumero"),
                        tulokset.GetString("paikkakunta")));
                }

                return asiakkaat;
            }
            catch (Exception e)
            {
                throw new Exception("Virhe asiakkaiden hakemisessa. ", e);
            }
        }
        


    }
}