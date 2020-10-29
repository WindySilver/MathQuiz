using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;


/// @author Noora Jokela ja Janne Taipalus
/// @version 29.10.2020
///  <summary>
///  Pelaajalle annetaan satunnaisesti valittu yhteen-, vähennys- tai kertolasku, joka hänen pitää ratkaista aikarajan sisällä. Oikeista vastauksista saa pisteitä, väärästä vastauksesta peli päättyy.
/// </summary>
public class MathQuiz : Game

{
    /// <summary>Lausekkeen tekijöiden korkein mahdollinen vaihtoehto.</summary>
    private const int KorkeinTekijaNumero = 100;
    /// <summary>Korkein arvottava numero.</summary>
    private const int KorkeinVastausNumero = 1000;
    /// <summary>Vastausvaihtoehtojen määrä "Ei mikään näistä" -vastausvaihtoehdon lisäksi.</summary>
    private const int VastaustenMaara = 3;
    ///<summary>Vakio sille, kuinka korkealla tekijälistan ensimmäinen rivi on.</summary>
    private const int TekijalistaKorkeus = 150;
    /// <summary>Vakio sille, millä kohtaa x-akselia kukin tekijälistan rivi on.</summary>
    private const int TekijalistaX = 0;
    ///<summary>Vakio tekijälistan rivien välille.</summary>
    private const int TekijalistanVali = 50;
    ///<summary>Oikea vastaus, jota kaikki sitä tarvitsevat aliohjelmat käyttävät.</summary>
    private double vastaus = 0;
    ///<summary>Vastausvaihtoehtojen taulukko, jota kaikki sitä tarvitsevat aliohjelmat käyttävät.</summary>
    private double[] vastausTaulukko = new double[] { 0, 0, 0, 0 };
    ///<summary>Ajastetun pelin pistelista.</summary>
    private EasyHighScore pistelista = new EasyHighScore();
    /// <summary>
    /// Ajastamattoman pelin pistelista
    /// </summary>
    private EasyHighScore ajastimetonPistelista = new EasyHighScore();
    ///<summary>Pistelaskuri.</summary>
    private DoubleMeter pistelaskuri = new DoubleMeter(0);
    ///<summary>Aikalaskuri.</summary>
    private DoubleMeter aikalaskuri = new DoubleMeter(30);
    /// <summary>Pelin päättyessä pelattava ääniefekti.</summary>
    private SoundEffect gameOver;
    /// <summary>
    /// Tosi, jos ajastin käytössä, false jos ei.
    /// </summary>
    private bool ajastin;


    /// <summary>
    /// Aloitetaan peli.
    /// </summary>
    public override void Begin()
    {
        ClearAll();
        LataaAanet();
        AloitaMusiikki();
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Alkuvalikko/Menu", "Aloita peli/Begin", "Pelaa ilman ajastinta/Play without a timer", "Pistelista/Point List", "Ajastamattoman tilan pistelista/Point list of the timerless mode", "Tekijät/Creators", "Lopeta/Exit");
        alkuvalikko.AddItemHandler(0, AloitaAjastinPeli);
        alkuvalikko.AddItemHandler(1, AloitaAjastimetonPeli);
        alkuvalikko.AddItemHandler(2, Pistelista);
        alkuvalikko.AddItemHandler(3, AjastamatonPistelista);
        alkuvalikko.AddItemHandler(4, Tekijat);
        alkuvalikko.AddItemHandler(5, Exit);
        Add(alkuvalikko);
   //     alkuvalikko.DefaultCancel = 5;
        Level.Background.Color = Color.White;
        LataaAanet();
        Camera.ZoomToLevel();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli/Exit");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli/Exit");
    }


    /// <summary>
    /// Aloitaa taustamusiikin soittamisen.
    /// </summary>
    private void AloitaMusiikki()
    {
        if (MediaPlayer.IsPlaying == true) return;
        MediaPlayer.Play("sb_anewyear");
        MediaPlayer.IsRepeating = true;
    }


    /// <summary>
    /// Lataa ääniefektit peliin.
    /// </summary>
    private void LataaAanet()
    {
        gameOver = LoadSoundEffect("GameOver");

    }


    /// <summary>
    /// Kertoo pelin ja sen sisällön tekijät.
    /// </summary>
    private void Tekijat()
    {
        ClearAll();
        List<Label> tekijaLista = new List<Label> ();

        Label ensimmainenRivi = new Label("Tekijät/Creators:");
        ensimmainenRivi.Color = RandomGen.NextColor();
        tekijaLista.Add(ensimmainenRivi);

        Label toinenRivi = new Label("Ohjelmointi/Programming:");
        toinenRivi.Color = RandomGen.NextColor();
        tekijaLista.Add(toinenRivi);

        Label kolmasRivi = new Label("Janne Taipalus");
        tekijaLista.Add(kolmasRivi);

        Label neljasRivi = new Label("Noora 'WindySilver' Jokela");
        tekijaLista.Add(neljasRivi);

        Label viidesRivi = new Label("Ääniefektit/Sound Effects:");
        viidesRivi.Color = RandomGen.NextColor();
        tekijaLista.Add(viidesRivi);

        Label kuudesRivi = new Label("Noora 'WindySilver' Jokela");
        tekijaLista.Add(kuudesRivi);

        Label seitsemasRivi = new Label("Musiikki/Music:");
        seitsemasRivi.Color = RandomGen.NextColor();
        tekijaLista.Add(seitsemasRivi);

        Label kahdeksasRivi = new Label("'A New Year' by Scott Buckley; Lisenssi/Licence: CC BY 4.0");
        tekijaLista.Add(kahdeksasRivi);

        for (int i = 0; i < tekijaLista.Count; i++)
        {
            tekijaLista[i].Position = new Vector(TekijalistaX, Level.Top - TekijalistaKorkeus - i * TekijalistanVali);
            Add(tekijaLista[i]);
        }

        MultiSelectWindow paluu = new MultiSelectWindow("Palaa alkuvalikkoon/Return to menu", "Palaa/Return");
        paluu.AddItemHandler(0, Begin);
        paluu.Color = Color.BlueGray;
        paluu.Position = new Vector(kahdeksasRivi.X, kahdeksasRivi.Y - TekijalistanVali*2);
        Add(paluu);
    }


    /// <summary>
    /// Näyttää pistelistan alkuvalikossa.
    /// </summary>
    private void Pistelista()
    {
        pistelista.Show();
        pistelista.HighScoreWindow.Closed += PaluuValikkoon;
    }

    /// <summary>
    /// Näyttää ajastamattoman pelin pistelistan alkuvalikossa.
    /// </summary>
    private void AjastamatonPistelista()
    {
        ajastimetonPistelista.Show();
        ajastimetonPistelista.HighScoreWindow.Closed += PaluuValikkoon;
    }

    /// <summary>
    /// Palauttaa pelaajan alkuvalikkoon.
    /// </summary>
    /// <param name="lahettaja">Ikkuna, joka lähettää pelaajan alkuvalikkoon.</param>
    private void PaluuValikkoon(Window lahettaja)
    {
        pistelista.HighScoreWindow.Closed -= PaluuValikkoon;
        Begin();
    }

    /// <summary>
    /// Aloittaa pelin ajastimen kanssa
    /// </summary>
    public void AloitaAjastinPeli()
    {
        ajastin = true;
        AloitaPeli();
    }

    /// <summary>
    /// Aloittaa pelin ilman ajastinta
    /// </summary>
    public void AloitaAjastimetonPeli()
    {
        ajastin = false;
        AloitaPeli();
    }

    /// <summary>
    /// Luo lausekkeen ja vastausvaihtoehdot.
    /// </summary>
    public void AloitaPeli()
    {
        ClearAll();
        LuoPistelaskuri();
        if (ajastin == true)
        {
            LuoAikaLaskuri();
            aikalaskuri.Reset();
        }
        int a = RandomGen.NextInt(KorkeinTekijaNumero);
        int b = RandomGen.NextInt(KorkeinTekijaNumero);

        string merkki = Merkki();

        switch (merkki)
        {
            case "+":
                {
                    vastaus = a + b;
                    break;
                }
            case "-":
                {
                    vastaus = a - b;
                    break;
                }
            case "*":
                {
                    vastaus = a * b;
                    break;
                }
            case "/":
                {
                    if (a % b != 0)
                    {
                        a = a * b;
                    }
                    vastaus = a / b;
                    break;
                }
        }

        Lauseke(a, b, merkki);
        Vaihtoehdot(vastaus);
    }


    /// <summary>
    /// Luo aikalaskurin.
    /// </summary>
    private void LuoAikaLaskuri()
    {
        Timer laskuri = Timer.CreateAndStart(0.1, LaskeAikaa);
        Label aikaNaytto = new Label();
        aikaNaytto.TextColor = Color.Black;
        aikaNaytto.Y = Level.Top - 100;
        aikaNaytto.X = Screen.Right - 100;
        aikaNaytto.DecimalPlaces = 2;
        aikaNaytto.BindTo(aikalaskuri);
        Add(aikaNaytto);
    }


    /// <summary>
    /// Laskee aikalaskurissa olevaa aikaa intervallin mukaan.
    /// </summary>
    private void LaskeAikaa()
    {
        aikalaskuri.Value -= 0.1;

        if (aikalaskuri.Value <= 0)
        {
            aikalaskuri.Stop();
            PeliOhi();
        }
    }


    /// <summary>
    /// Tulostaa laskutoimituksen lausekkeen näytölle.
    /// </summary>
    /// <param name="a">Laskutoimituksen toinen puoli</param>
    /// <param name="b">Laskutoimituksen toinen puoli</param>
    /// <param name="merkki">Minkä merkkinen laskutoimitus on.</param>
    public void Lauseke(double a, double b, string merkki)
    {
        Label lauseke = new Label(200.0, 50.0);
        lauseke.Y = Level.Top - 100;
        lauseke.Text = a + " " + merkki + " " + b + " = ?";
        Add(lauseke);
    }


    /// <summary>
    /// Luo valintaikkunan, jossa on vastausvaihtoehdot.
    /// </summary>
    /// <param name="vastaus">Oikea vastaus.</param>
    public void Vaihtoehdot(double vastaus)
    {
        vastausTaulukko = ArvoVaihtoehdot(vastaus);
        MultiSelectWindow vaihtoehdot = new MultiSelectWindow("Vaihtoehdot/Options", vastausTaulukko[0].ToString(), vastausTaulukko[1].ToString(), vastausTaulukko[2].ToString(), "Ei mikään näistä/None of these");
        vaihtoehdot.ItemSelected += PainettiinVaihtoehtoa;
        Add(vaihtoehdot);
    }


    /// <summary>
    /// Valitsee, mikä laskutoimitus tehdään.
    /// </summary>
    /// <returns>Laskutoimituksen merkin</returns>
    public static string Merkki()
    {
        string merkki = RandomGen.SelectOne("+","-","*","/");
        return merkki;
    }


    /// <summary>
    /// Tarkistaa, onko pelaajan valitsema vaihtoehto oikein.
    /// </summary>
    /// <param name="valinta">Valittu vaihtoehto.</param>
    public void PainettiinVaihtoehtoa(int valinta)
    {
        if (vastausTaulukko[valinta] == vastaus)
        {
            pistelaskuri.Value += 1;
            AloitaPeli();
        }
        else PeliOhi();
    }


    /// <summary>
    /// Luo pistelaskurin pelille.
    /// </summary>
    private void LuoPistelaskuri()
    {
        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + 100;
        pisteNaytto.Y = Screen.Top - 100;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = "Pisteet/Points";
        pisteNaytto.BindTo(pistelaskuri);
        Add(pisteNaytto);
    }


    /// <summary>
    /// Suoritetaan, kun peli päättyy.
    /// </summary>
    public void PeliOhi()
    {
        ClearAll();
        gameOver.Play();
        MultiSelectWindow lopetusikkuna = new MultiSelectWindow("", "Aloita uudelleen/Restart", "Palaa alkuvalikkoon/Return to menu");
        lopetusikkuna.AddItemHandler(0, AloitaPeli);
        lopetusikkuna.AddItemHandler(1, Begin);
        Add(lopetusikkuna);
        Label peliOhi = new Label("Peli päättyi!/Game over!");
        peliOhi.Y = Level.Top - 200;
        Add(peliOhi);
        if (ajastin == true) pistelista.EnterAndShow(pistelaskuri.Value);
        else ajastimetonPistelista.EnterAndShow(pistelaskuri.Value);
        pistelaskuri.Value = 0;
    }


    /// <summary>
    /// Arpoo vastausvaihtoehdot.
    /// </summary>
    /// <param name="vastaus">Vastaus, joka laitetaan, jos jokin on oikea.</param>
    /// <returns>Taulukko, jossa on vastaukset.</returns>
    public static double[] ArvoVaihtoehdot(double vastaus)
    {
        int oikea = VastaustenMaara;
        double[] taulukko = new double[] { 0, 0, 0, 0 };
        if (RandomGen.NextBool() == true)
        {
            oikea = RandomGen.NextInt(VastaustenMaara);
            for (int i = 0; i <= VastaustenMaara; i++)
            {
                if (i == oikea) taulukko[i] = vastaus;
                else taulukko[i] = RandomGen.NextInt(KorkeinVastausNumero);
            }
        }
        else
        {
            taulukko[VastaustenMaara] = vastaus;
            for (int i = 0; i <=VastaustenMaara-1; i++)
            {
               taulukko[i] = RandomGen.NextInt(KorkeinVastausNumero);
            }
        }
        return taulukko;
    }
}