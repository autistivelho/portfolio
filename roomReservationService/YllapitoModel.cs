using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

namespace RavintolaTalliYllapito.Models
{
    public class YllapitoModel : Tietokantayhteys
    {

        /// <summary>
        /// Hakee tietokannassa olevat varaukset. Palauttaa listan Varaus-objekteja.
        /// </summary>
        /// <returns></returns>
        public List<Varaus> HaeKaikkiVaraukset()
        {
            try
            {
                const string sqlLause = "SELECT varaukset.id, varaukset.asiakasID, asiakkaat.etunimi, asiakkaat.sukunimi, asiakkaat.sahkoposti, asiakkaat.puh, asiakkaat.katuosoite, asiakkaat.postinumero, asiakkaat.paikkakunta, varaukset.tilaID, tilat.nimi, tilat.katuosoite, tilat.paikkakunta, tilat.rakennus, tilat.varustus, tilat.kapasiteetti, tilat.hinta, varaukset.aloituspvm, varaukset.lopetuspvm, varaukset.maksutapa, varaukset.summa, varaukset.henkilomaara, varaukset.lisapalvelut FROM varaukset JOIN asiakkaat ON varaukset.asiakasID = asiakkaat.id JOIN tilat ON varaukset.tilaID = tilat.id ORDER BY varaukset.aloituspvm DESC;";

                var tulokset = new MySqlCommand(sqlLause, Yhteys).ExecuteReader();

                var varaukset = new List<Varaus>();

                while (tulokset.Read())
                {
                    varaukset.Add(new Varaus(
                        tulokset.GetInt32("id"),
                        new Asiakas(tulokset.GetString("etunimi"),
                            tulokset.GetString("sukunimi"),
                            tulokset.GetString("sahkoposti"),
                            tulokset.GetString("puh"),
                            tulokset.GetString("katuosoite"),
                            tulokset.GetInt32("postinumero"),
                            tulokset.GetString("paikkakunta")),
                        new Tila(tulokset.GetInt32("tilaID"),
                            tulokset.GetString("nimi"),
                            tulokset.GetString("katuosoite"),
                            tulokset.GetString("paikkakunta"),
                            tulokset.GetString("rakennus"),
                            tulokset.GetString("varustus"),
                            tulokset.GetInt32("kapasiteetti"),
                            tulokset.GetInt32("hinta")), 
                        tulokset.GetInt64("aloituspvm"),
                        tulokset.GetInt64("lopetuspvm"),
                        tulokset.GetString("maksutapa"),
                        tulokset.GetInt32("summa"),
                        tulokset.GetInt32("henkilomaara"),
                        tulokset.GetString("lisapalvelut")));
                }

                tulokset.Close();

                return varaukset;
            }
            catch (Exception e)
            {
                throw new Exception("Virhe varausten hakemisessa. ", e);
            }
        }

        /// <summary>
        /// Hakee kaikki tilat tai varauksessa olevan tilan, palauttaa listan tiloista.
        /// </summary>
        /// <returns></returns>
        public List<Tila> HaeTilat()
        {
            try
            {
                var sqlLause = "SELECT * FROM tilat;";
                var komento = new MySqlCommand(sqlLause, Yhteys);

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

        /// <summary>
        /// Hakee kaikki asiakkaat tietokannasta. Palauttaa listan Asiakas-objekteja.
        /// </summary>
        /// <returns></returns>
        public List<Asiakas> HaeKaikkiAsiakkaat()
        {
            try
            {
                const string sqlLause = "SELECT * FROM asiakkaat;";

                var tulokset = new MySqlCommand(sqlLause, Yhteys).ExecuteReader();

                var asiakkaat = new List<Asiakas>();

                while (tulokset.Read())
                {
                    asiakkaat.Add(new Asiakas(tulokset.GetString("etunimi"),
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


        /// <summary>
        /// </summary>
        /// <param name="asiakas"></param>
        /// <param name="tilaId"></param>
        /// <param name="henkilomaara"></param>
        /// <param name="lisapalvelut"></param>
        /// <param name="aloitusPvm"></param>
        /// <param name="lopetusPvm"></param>
        /// <param name="maksutapa"></param>
        /// <param name="summa"></param>
        /// <param name="asiakkaat"></param>
        /// <returns></returns>
        public List<Asiakas> LisaaVaraus(Asiakas asiakas, int tilaId, int henkilomaara, string lisapalvelut, long aloitusPvm, long lopetusPvm, string maksutapa, int summa, List<Asiakas> asiakkaat)
        {
            try
            {
                if (!asiakkaat.Contains(asiakas))
                {
                    asiakkaat = LisaaAsiakas(asiakas, asiakkaat);
                }

                var asiakasId = asiakkaat.FindIndex(x => x == asiakas) + 1;

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

                return asiakkaat;
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


        public bool PoistaVaraus(int varausId)
        {
            try
            {
                const string sqlLause = "DELETE FROM varaukset WHERE id = @varausId;";

                var komento = new MySqlCommand(sqlLause, Yhteys);
                komento.Parameters.Add("@varausId", MySqlDbType.Int32).Value = varausId;

                komento.ExecuteNonQuery();

                return true;
            }
            catch (Exception e)
            {
                throw new Exception("Virhe varauksen poistamisessa. ", e);
            }
            
        }
    }
}