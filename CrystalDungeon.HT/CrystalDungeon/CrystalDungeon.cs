using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

/// @author gr313135
/// @version 18.11.2025
/// <summary>
/// Tasohyppelypeli, jossa väistellään esteitä, tuhotaan vihuja, kerätään kristalleja ja pyritään voittamaan pomo.
/// Saat pisteitä keräämällä kristalleja, tuhoamalla vihuja, vahingoittamalla pomoa ja keräämällä terveyspisteitä.
/// </summary>


public class CrystalDungeon : PhysicsGame
{
    private const int Liikenopeus = 200;
    private const int Hyppynopeus = 650;
    private const int RuudunKoko = 40;
    private const int MaxTerveys = 10;
    private const int PomoTerveys = 100;
    private PlatformCharacter pelaaja1;
    private PlatformCharacter vihollinen;
    private PlatformCharacter pomo;
    private List<PlatformCharacter> vihut = new List<PlatformCharacter>();
    private List<PhysicsObject> kristallit = new List<PhysicsObject>();
    private PhysicsObject kristalli;
    private Image[] kristalliAnimaatio = LoadImages("Kristalli1", "Kristalli2", "Kristalli3", "Kristalli4", "Kristalli5", "Kristalli6", "Kristalli7", "Kristalli8");
    private Image[] vihuAnimaatio = LoadImages("Vihu1", "Vihu2", "Vihu3");
    private Image pelaajan1Kuva = LoadImage("pelaaja.png");
    private Image tippukiviKuva = LoadImage("Tippukivi.png");
    private Image piikkiKuva = LoadImage("Piikki.png");
    private Image liikkuvaTasoKuva = LoadImage("Leijuvakivi.png");
    private Image tyhjaPomopalkki = LoadImage("tyhjapalkki");
    private Image taysiPomopalkki = LoadImage("taysipalkki");
    private Image isoPiikkiKuva = LoadImage("Piikki2");
    private Image murskainKuva = LoadImage("Murskain");
    private Image aseKuva = LoadImage("sauva");
    private Image pomoAseKuva = LoadImage("Pomoase");
    private SoundEffect maaliAani = LoadSoundEffect("maali.wav");
    private IntMeter pistelaskuri;
    private IntMeter terveyslaskuri;
    private IntMeter pomoLaskuri;
    private Timer aikalaskuri;
    private EasyHighScore topLista = new EasyHighScore();
    private AssaultRifle pelaaja1Ase;
    private Cannon pomoAse;
    private Cannon pomoAse2;
    private int kerroin = 1;
    private int muutetut;
    
    public override void Begin()
    {
        AloitaAlusta();
    }
    
    
    private void AloitaPeli(Window sender)
    {
        AloitaAlusta();
    }
    

    /// <summary>
    /// Aloittaa pelin kutsumalla kaikki tarpeelliset aliohjelmat.
    /// </summary>
    private void AloitaAlusta()
    {
        ClearAll(); // poistaa kaiken, jotta peli voidaan aloittaa alusta
        LuoKentta();
        LisaaNappaimet();
        LuoPistelaskuri();
        LuoTerveysLaskuri();
        LuoAikalaskuri();
    }
    
    /// <summary>
    /// Luo kentän tekstiedoston pohjalta ja alustaa toimintoja.
    /// </summary>
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('¤', LisaaTaso2);
        kentta.SetTileMethod('*', LisaaKristalli);
        kentta.SetTileMethod('S', LisaaSydan);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.SetTileMethod('^', LisaaPiikki);
        kentta.SetTileMethod('L', LisaaLiikkuvaTaso);
        kentta.SetTileMethod('M',LisaaMurskain);
        kentta.SetTileMethod('I', LisaaIsoPiikki);
        kentta.SetTileMethod('T', LisaaTippukivi);
        kentta.SetTileMethod('V', LisaaVihollinen);
        kentta.SetTileMethod('P', LisaaPomo);
        kentta.Execute(RuudunKoko, RuudunKoko);
        Level.CreateBorders();
        Level.BackgroundColor = Color.White;
        
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1;
        Camera.StayInLevel = true;
        MasterVolume = 0.5;
        Gravity = new Vector(0, -1000);
    }
    

    /// <summary>
    /// Aliohjelman avulla saadan vähennettyä toistoa koodissa tekemällä aliohjelma,
    /// joka tekee kaikille objekteille yhteiset asiat vaihtuvilla parametreilla.
    /// </summary>
    private PhysicsObject LuoRakenne(Vector kohta, double leveys, double korkeus, Shape muoto, Color vari)
    {
        PhysicsObject objekti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        objekti.Color = vari;
        objekti.Shape = muoto;
        objekti.Position = kohta;
        objekti.Tag = "taso";
        return objekti;
    }


    /// <summary>
    /// Saadan tälläkin poistettua toistoa luomalla labeleita hyödyntäen aliohjelmaa.
    /// </summary>
    private Label LuoLabeli(double x, double y, Color tekstivari, Color taustavari, string otsikko)
    {
        Label labeli = new Label();
        labeli.X = Screen.Right - x;
        labeli.Y = Screen.Top - y;
        labeli.TextColor = tekstivari;
        labeli.Color = taustavari;
        labeli.Title = otsikko;
        return labeli;
    }

    
    private void LisaaVihollinen(Vector paikka, double leveys, double korkeus)
    {
        vihollinen = LuoVihollinen(paikka, leveys, korkeus);
        Add(vihollinen);
    }


    private PlatformCharacter LuoVihollinen(Vector paikka, double leveys, double korkeus)
    {
        vihollinen = new PlatformCharacter(leveys*2, korkeus*2);
        vihollinen.Position = paikka;
        vihollinen.Mass = 5.0;
        vihollinen.Shape = Shape.Circle;
        vihollinen.Color = Color.Green;
        vihollinen.Tag = "vihollinen";
        PlatformWandererBrain tasoaivot = new PlatformWandererBrain(); //Saadaan vihu liikkumaan kentällä
        tasoaivot.Speed = 70;
        vihollinen.Brain = tasoaivot;
        vihollinen.Animation = new Animation(vihuAnimaatio);
        vihollinen.Animation.Start();
        vihollinen.Animation.FPS = 3;
        
        return vihollinen;
    }
    
    
    private void Muutavihut()
    {
        if (muutetut < kristallit.Count)
        {
            PlatformCharacter muutettuvihu = LuoVihollinen(kristallit[muutetut].Position, kristalli.Width, kristalli.Height);
            Add(muutettuvihu);
            vihut.Add(muutettuvihu);
            kristallit[muutetut].Destroy();
            muutetut++;
        }
    }
    
    
    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        GameObject taso = new GameObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.DarkGray;
        Add(taso);
    }
    
    
    private void LisaaTaso2(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso2 = LuoRakenne(paikka, leveys, korkeus, Shape.Rectangle, Color.DarkGray);
        Add(taso2);
    }
    
    
    private void LisaaPiikki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject piikki = LuoRakenne(paikka, leveys, korkeus, Shape.Triangle, Color.HotPink);
        piikki.Image = piikkiKuva;
        piikki.Tag = "piikki";
        Add(piikki);
    }
    
    
    private void LisaaIsoPiikki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject isoPiikki = LuoRakenne(paikka, leveys, korkeus, Shape.Triangle, Color.BloodRed);
        isoPiikki.Tag = "tappava";
        isoPiikki.Image = isoPiikkiKuva;
        Add(isoPiikki);
    }
    
    
    private void LisaaTippukivi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tippukivi = LuoRakenne(paikka, leveys, korkeus, Shape.Rectangle, Color.Wheat);
        tippukivi.Tag = "tappava";
        tippukivi.Image = tippukiviKuva;
        Add(tippukivi);
    }
    

    private void LisaaKristalli(Vector paikka, double leveys, double korkeus)
    {
        kristalli = LuoRakenne(paikka, leveys, korkeus, Shape.Rectangle, Color.HotPink);
        kristalli.IgnoresCollisionResponse = true;
        kristalli.Animation = new Animation(kristalliAnimaatio);
        kristalli.Animation.Start();
        kristalli.Animation.FPS = 8;
        kristalli.Tag = "kristalli";
        kristallit.Add(kristalli);
        Add(kristalli);
    }
    
    
    private void LisaaSydan(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject sydan = LuoRakenne(paikka, leveys, korkeus, Shape.Heart, Color.MediumBlue);
        sydan.IgnoresCollisionResponse = true;
        sydan.Tag = "sydan";
        Add(sydan);
    }
    
    
    private void LisaaLiikkuvaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject liikkuvataso = LuoRakenne(paikka, leveys, korkeus/2, Shape.Rectangle, Color.Charcoal);
        liikkuvataso.IgnoresGravity = true;
        liikkuvataso.Oscillate(Vector.UnitY, 150, 0.2); //Tekee liikkeen värähtelemällä
        liikkuvataso.Image = liikkuvaTasoKuva;
        Add(liikkuvataso);
    }
    
    
    private void LisaaMurskain(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject murskain = LuoRakenne(paikka, leveys*1.9, korkeus*1.9, Shape.Rectangle, Color.Black);
        murskain.X += 21;
        murskain.Y -= 20;
        murskain.IgnoresGravity = true;
        murskain.Oscillate(Vector.UnitY, 160, 0.1); //Tekee liikkeen värähtelemällä
        murskain.Tag = "tappava";
        murskain.Image = murskainKuva;
        Add(murskain);
    }
    

    public void LuoPistelaskuri()
    {
        pistelaskuri = new IntMeter(0);
        
        Label pistetaulukko = LuoLabeli(100, 50, Color.Black, Color.White, "Pisteet: ");
        pistetaulukko.BindTo(pistelaskuri);
        Add(pistetaulukko);
    }
    
    
    public void LuoTerveysLaskuri()
    {
        terveyslaskuri= new IntMeter(MaxTerveys/2, 0, MaxTerveys);
        terveyslaskuri.LowerLimit += PelaajaKuoli; //Kun laskuri saa pienimmän arvon eli 0, pelaaja kuolee.
        
        Label pelaajanTerveys = LuoLabeli(100, 70, Color.Black, Color.White, "Terveys: ");
        pelaajanTerveys.BindTo(terveyslaskuri);
        Add(pelaajanTerveys);
        
        ProgressBar elamapalkki = new ProgressBar(150, 20);
        elamapalkki.X = Screen.Right - 100;
        elamapalkki.Y = Screen.Top - 100;
        elamapalkki.BarColor = Color.Azure;
        elamapalkki.Color = Color.AshGray;
        elamapalkki.BorderColor = Color.Black;
        elamapalkki.Angle = Angle.FromDegrees(180);
        elamapalkki.BindTo(terveyslaskuri);
        Add(elamapalkki);
    }
    
    
    public void LuoAikalaskuri()
    {
        aikalaskuri = new Timer();
        aikalaskuri.Start();

        Label aikanaytto = LuoLabeli(100, 30, Color.Black, Color.White, "Aika: ");
        aikanaytto.DecimalPlaces = 1;
        aikanaytto.BindTo(aikalaskuri.SecondCounter);
        Add(aikanaytto);
    }
    
    
    /// <summary>
    /// Luo pelaajan, lisää sille aseen ja luo tarvittavat pelaajaan liittyvät törmäyksenkäsittelijät. 
    /// </summary>
    /// <param name="paikka">Paikka, johon pelaaja syntyy, tässä tapauksessa määritelty kentän tekstitiedostossa.</param>
    /// <param name="leveys">Pelaajan leveys.</param>
    /// <param name="korkeus">Pelaajan korkeus.</param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajan1Kuva;
        pelaaja1.IgnoresCollisionResponse = false;
        
        pelaaja1Ase = new AssaultRifle(25, 25);
        pelaaja1.Add(pelaaja1Ase);
        pelaaja1Ase.FireRate = 3;
        pelaaja1Ase.Position = pelaaja1.Position + new Vector(pelaaja1.Width / 2.5, 0);
        pelaaja1Ase.MaxAmmoLifetime = TimeSpan.FromSeconds(3);
        pelaaja1Ase.Image = aseKuva;
        
        AddCollisionHandler(pelaaja1, "kristalli", KeraaKristalli);
        AddCollisionHandler(pelaaja1, "piikki", Osui);
        AddCollisionHandler(pelaaja1, "sydan", KeraaSydan);
        AddCollisionHandler(pelaaja1, "tappava", OsuiKuolettavasti);
        AddCollisionHandler(pelaaja1, "vihollinen", Osui);
        AddCollisionHandler(pelaaja1, "pomo", Osui);
        Add(pelaaja1);
    }


    /// <summary>
    /// Luo pomon, lisäten sille aseet, aivot ja pomon terveyslaskurin sekä -palkin.
    /// Aivoilla saadaan pomo liikkumaan kentällä.
    /// Pomon terveys tehdään kokonaislukuja käyttävällä intmeter-laskurilla.
    /// Terveyspalkki liitetään pomon terveyteen, jotta se vähenee pomon terveyden vähetessä intmeterissä.
    /// </summary>
    /// <param name="paikka">Paikka, johon pomo syntyy, tässä tapauksessa määritelty kentän tekstitiedostossa.</param>
    /// <param name="leveys">Pomon leveys.</param>
    /// <param name="korkeus">Pomon korkeus.</param>
    private void LisaaPomo(Vector paikka, double leveys, double korkeus)
    {
        pomo = new PlatformCharacter(leveys*2, korkeus*2);
        pomo.Position = paikka;
        pomo.Mass = 5.0;
        pomo.Shape = Shape.Circle;
        pomo.Color = Color.Green;
        pomo.Tag = "pomo";
        Add(pomo);

        //Aivojen tekeminen, jolla saadan pomo liikkumaan kentällä.
        PlatformWandererBrain pomoaivot = new PlatformWandererBrain();
        pomoaivot.FallsOffPlatforms = true;
        pomoaivot.TriesToJump = true;
        pomoaivot.JumpSpeed = 100;
        pomoaivot.Speed = 50;
        pomo.Brain = pomoaivot;
        
        pomoAse = new Cannon(30, 20);
        pomo.Add(pomoAse);
        pomoAse.Image = pomoAseKuva;
        pomoAse.FireRate = 0.4;
        pomoAse.Position = pomo.Position + new Vector(pomo.Width / 2, 0);
        pomoAse.MaxAmmoLifetime = TimeSpan.FromSeconds(2.5);
        pomoAse.CanHitOwner = true;
        
        pomoAse2 = new Cannon(30, 20);
        pomo.Add(pomoAse2);
        pomoAse2.Image = pomoAseKuva;
        pomoAse2.FireRate = 0.4;
        pomoAse2.Position = pomo.Position + new Vector(pomo.Width / -2, 0);
        pomoAse2.MaxAmmoLifetime = TimeSpan.FromSeconds(2.5);
        pomoAse2.Angle = Angle.FromDegrees(180);
        pomoAse2.CanHitOwner = true;
        
        pomoLaskuri= new IntMeter(PomoTerveys, 0, PomoTerveys);
        pomoLaskuri.LowerLimit += PomoKuoli; //Kun laskuri saa pienimmän arvon eli 0, pomo kuolee.
        pomoLaskuri.AddTrigger(25, TriggerDirection.Down, Tuhoavihut);
        
        ProgressBar pomopalkki = new ProgressBar(500, 60);
        pomopalkki.X = 0;
        pomopalkki.Y = Screen.Top - 25;
        pomopalkki.BarImage = tyhjaPomopalkki;
        pomopalkki.Image = taysiPomopalkki;
        pomopalkki.BarColor = Color.Red;
        pomopalkki.Color = Color.Black;
        pomopalkki.BorderColor = Color.Black;
        pomopalkki.Angle = Angle.FromDegrees(180);
        pomopalkki.BindTo(pomoLaskuri);
        Add(pomopalkki);
    }


    private void Tuhoavihut()
    {
        int vihujenmaara = vihut.Count;

        if (vihujenmaara > 0)
        {
            for (int i = 0; i < vihujenmaara; i++)
            {
                vihut[i].Destroy();
            }
        }
    }
    

    private void LisaaNappaimet()
    {
        //Liikkuminen:
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -Liikenopeus);
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, Liikenopeus);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, Hyppynopeus);
        Keyboard.Listen(Key.S, ButtonState.Pressed, Hyppaa, "", pelaaja1, -Hyppynopeus);
        Keyboard.Listen(Key.Space, ButtonState.Down, Hyppaa, "", pelaaja1, Hyppynopeus);
        
        //Muut näppäimet:
        Keyboard.Listen(Key.R, ButtonState.Down, AloitaAlusta, "Aloittaa pelin alusta"); //Ei tarvitse ajaa peliä uudelleen, jos haluaa aloittaa alusta
        Keyboard.Listen(Key.LeftShift, ButtonState.Pressed, AktivoiHuijaukset, "");
        Mouse.Listen(MouseButton.Left, ButtonState.Down, Ammu, "Kutsuu loitsun");
        Keyboard.Listen(Key.H, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
    

    /// <summary>
    /// Liikuttaaa pelaajaa painetun näppäimen mukaan oikeaan suuntaan käyttäen valmiiksi määriteltyjä liikenopeutta. 
    /// </summary>
    /// <param name="hahmo">Kohde, jota halutaan liikuttaa näppäimillä, tässä tapauksessa pelaaja.</param>
    /// <param name="nopeus">Kohteen liikkeen suunta ja suuruus.</param>
    private void Liikuta(PlatformCharacter hahmo, int nopeus)
    {
        hahmo.Walk(nopeus);
        pelaaja1Ase.Angle = pelaaja1.Velocity.Angle;
    }
    

    /// <summary>
    /// Pelaajaa hyppää painetun näppäimen mukaan käyttäen valmiiksi määriteltyjä hyppynopeutta. 
    /// </summary>
    /// <param name="hahmo">Kohde, jota halutaan liikuttaa näppäimillä, tässä tapauksessa pelaaja.</param>
    /// <param name="nopeus">Kohteen liikkeen suunta ja suuruus.</param>
    private void Hyppaa(PlatformCharacter hahmo, int nopeus)
    {
        hahmo.Jump(nopeus);
    }
    
    
    private void Ammu()
    {
        PhysicsObject ammus = pelaaja1Ase.Shoot();
        if (ammus != null) //Peli saattoi kaatua, jos ei tehnyt tarkistusta ja collisionhandler aktivoitui.
        {
            ammus.Tag = "Luoti";
            AddCollisionHandler<PhysicsObject, PlatformCharacter>(ammus, "vihollinen", AmmusOsui);
            AddCollisionHandler<PhysicsObject, PlatformCharacter>(ammus, "pomo", AmmusOsui);
            AddCollisionHandler<PhysicsObject, PhysicsObject>(ammus, "taso", AmmusOsui);
        }
    }


    private void PomoAmpuu()
    {
        PhysicsObject kuula = pomoAse.Shoot();
        PhysicsObject kuula2 = pomoAse2.Shoot();
        if (kuula != null) //Peli saattoi kaatua, jos ei tehnyt tarkistusta ja collisionhandler aktivoitui.
        {
            kuula.Tag = "Kuula";
            AddCollisionHandler<PlatformCharacter, PhysicsObject>(pelaaja1, kuula, Osui);
        }
        
        if (kuula2 != null) //Peli saattoi kaatua, jos ei tehnyt tarkistusta ja collisionhandler aktivoitui.
        {
            kuula2.Tag = "Kuula2";
            AddCollisionHandler<PlatformCharacter, PhysicsObject>(pelaaja1, kuula2, Osui);
        }
    }
    

    private void KeraaKristalli(PhysicsObject hahmo, PhysicsObject kristallikohde)
    {
        maaliAani.Play();
        MessageDisplay.Add("Keräsit kristallin!");
        pistelaskuri.AddOverTime(500*kerroin, 0.5);
        kristallikohde.Destroy();
    }
    
    
    private void KeraaSydan(PhysicsObject hahmo, PhysicsObject sydan)
    {
        maaliAani.Play();
        MessageDisplay.Add("Keräsit sydämmen!");
        terveyslaskuri.AddOverTime(1, 0.5);
        pistelaskuri.AddOverTime(250 * kerroin, 0.5);
        sydan.Destroy();
    }
    
    
    private void Osui(PhysicsObject hahmo, PhysicsObject osuttu)
    {
        MessageDisplay.Add("Autss...");
        terveyslaskuri.AddValue(-1);
    }
    
    
    private void OsuiKuolettavasti(PhysicsObject hahmo, PhysicsObject tappava)
    {
        MessageDisplay.Add("Kuolit");
        terveyslaskuri.Value = 0;
    }
    
    
    private void PelaajaKuoli()
    {
        pelaaja1.Destroy();
        topLista.EnterAndShow(pistelaskuri.Value);
        topLista.HighScoreWindow.Closed += AloitaPeli;
    }
    

    private void PomoKuoli()
    {
        pomo.Destroy();
        pistelaskuri.AddOverTime(kerroin*10000, 10);
        VoititPelin();
    }
    
    
    private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        MessageDisplay.Add("Ammus osui: " + kohde.Tag);

        //Varmistetaan, että osuttu kohde on oikea ja saadaan tehtyä oikeat ohjelmat
        if (kohde.Tag.ToString() == "vihollinen")
        {
            kohde.Destroy();
            Muutavihut();
            pistelaskuri.AddOverTime(500 * kerroin, 0.5);
            ammus.Destroy();
        }

        //Varmistetaan, että osuttu kohde on oikea ja saadaan tehtyä oikeat ohjelmat
        if (kohde.Tag.ToString() == "pomo")
        {
            PomoAmpuu();
            pomoLaskuri.AddValue(-1);
            pistelaskuri.AddOverTime(100 * kerroin, 0.5);
            ammus.Destroy();
            Muutavihut();
        }

        if (kohde.Tag.ToString() == "taso")
        {
            ammus.Destroy();
        }
    }
    

    private void AktivoiHuijaukset()
    {
        kerroin = 1000;
        pelaaja1.IgnoresGravity = true;
    }
    
    
    /// <summary>
    /// Laskee pisteet kaikista kerätyistä asioista ja peliajasta, kun pomo on tapettu.
    /// </summary>
    public void VoititPelin()
    {
        Label voittoOtsikko = LuoLabeli(200, 0, Color.Gold, Color.Transparent, "Onneksi olkoon, Voitit pelin!");
        Add(voittoOtsikko);
        
        double pelattuAika = aikalaskuri.SecondCounter.Value;
        double jaanytTerveys = terveyslaskuri.Value * 10;
        double peliAika = 1000 / pelattuAika; //Mitä nopeammin pelattu, sitä enemmän saadaan pisteitä
        pistelaskuri.MultiplyValue(peliAika);
        pistelaskuri.MultiplyValue(jaanytTerveys);
        pelaaja1.Destroy();
        topLista.EnterAndShow(pistelaskuri.Value);
        topLista.HighScoreWindow.Closed += AloitaPeli;
    }
}