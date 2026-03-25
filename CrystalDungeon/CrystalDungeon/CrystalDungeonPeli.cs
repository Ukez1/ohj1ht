using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

namespace CrystalDungeon.HT
{
    /// @author gr313135
/// @version 18.11.2025
/// <summary>
/// Tasohyppelypeli, jossa väistellään esteitä, tuhotaan vihuja, kerätään kristalleja ja pyritään voittamaan pomo.
/// Saat pisteitä keräämällä kristalleja, tuhoamalla vihuja, vahingoittamalla pomoa ja keräämällä terveyspisteitä.
/// </summary>
public class CrystalDungeonPeli : PhysicsGame
{
    private const int Liikenopeus = 200;
    private const int Hyppynopeus = 650;
    private const int RuudunKoko = 40;
    private const int MaxTerveys = 10;
    private const int PomoTerveys = 100;
    private const int Sydanarvo = 250;
    private const int Kristalliarvo = 500;
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
    private Image isoPiikkiKuva = LoadImage("Piikki2");
    private Image murskainKuva = LoadImage("Murskain");
    private Image aseKuva1 = LoadImage("sauva1");
    private Image aseKuva2 = LoadImage("sauva2");
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
    private int asevahinko = 1;
    private bool asekeratty;
    
    public override void Begin()
    {
        AloitaPeli();
    }

    
    /// <summary>
    /// Luo alkuvalikon, josta pääsee näkemään top10-listan, aloittamaan pelin ja lopettamaan pelaamisen.
    /// </summary>
    private void AloitaPeli(Window sender=null)
    {
        ClearAll();
        Level.Background.Color = Color.LightGray;
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Crystal Dungeon", "Aloita peli", "Parhaat pisteet", "Lopeta");
        alkuvalikko.Font = Font.DefaultBold;
        alkuvalikko.BorderColor = Color.Azure;
        alkuvalikko.Color = Color.Transparent;
        alkuvalikko.SetButtonColor(Color.DarkAzure);
        alkuvalikko.SetButtonTextColor(Color.White);
        alkuvalikko.CapturesMouse = false;
        Add(alkuvalikko);
        alkuvalikko.AddItemHandler(0, AloitaAlusta);
        alkuvalikko.AddItemHandler(1, topLista.Show);
        alkuvalikko.AddItemHandler(2, Exit);
        topLista.Text = "Parhaat pisteet:";
        topLista.Color = Color.Azure;
        topLista.HighScoreWindow.Closed += AloitaPeli; //Kun top10-lista suljetaan, mennään takaisin alkuvalikkoon.
    }
    

    /// <summary>
    /// Aloittaa pelin kutsumalla kaikki tarpeelliset aliohjelmat pelin pelaamiseen.
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
        kentta.SetTileMethod('1', LisaaAse1);
        kentta.Execute(RuudunKoko, RuudunKoko);
        Level.CreateBorders();
        Level.BackgroundColor = Color.White;
        
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
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
    
    private void LisaaAse1(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject ase1 = LuoRakenne(paikka, leveys, korkeus, Shape.Heart, Color.MediumBlue);
        ase1.IgnoresCollisionResponse = true;
        ase1.Tag = "ase1";
        ase1.Image = aseKuva1;
        Add(ase1);
    }
    
    
    private void LisaaLiikkuvaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject liikkuvataso = LuoRakenne(paikka, leveys*2, korkeus/2, Shape.Rectangle, Color.Charcoal);
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
    

    /// <summary>
    /// Luo pistelaskurin, joka laskee pisteitä tuhoamalla vihuja, keräämällä kristalleja ja sydämiä. 
    /// </summary>
    public void LuoPistelaskuri()
    {
        pistelaskuri = new IntMeter(0);
        
        Label pistetaulukko = LuoLabeli(100, 50, Color.Black, Color.White, "Pisteet: ");
        pistetaulukko.BindTo(pistelaskuri);
        Add(pistetaulukko);
    }
    
    
    /// <summary>
    /// Luo terveyslaskurin, jonka arvo kertoo pelaajan terveyden. Terveyslaskurin arvo vähenee, kun pelaaja osuu piikkeihin tai vihuihin.
    /// Terveyttä saa lisää keräämällä sydämiä. Kun laskurin arvo menee 0, pelaaja kuolee.
    /// </summary>
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
    
    
    /// <summary>
    /// Luo aikalaskurin, joka kertoo kaunko peliä on pelattu kyseisellä yrityksellä.
    /// </summary>
    public void LuoAikalaskuri()
    {
        aikalaskuri = new Timer();
        aikalaskuri.Start();

        Label aikanaytto = LuoLabeli(100, 30, Color.Black, Color.White, "Aika: ");
        aikanaytto.DecimalPlaces = 1;
        aikanaytto.BindTo(aikalaskuri.SecondCounter);
        Add(aikanaytto);
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
        
        LuoAse(3, 1, aseKuva1);
        pelaaja1Ase.Image = aseKuva1;
        
        AddCollisionHandler(pelaaja1, "kristalli", Keraa);
        AddCollisionHandler(pelaaja1, "piikki", Osui);
        AddCollisionHandler(pelaaja1, "sydan", Keraa);
        AddCollisionHandler(pelaaja1, "tappava", OsuiKuolettavasti);
        AddCollisionHandler(pelaaja1, "vihollinen", Osui);
        AddCollisionHandler(pelaaja1, "pomo", Osui);
        AddCollisionHandler(pelaaja1, "ase1", Keraa);
        Add(pelaaja1);
    }


    /// <summary>
    /// Luo pelaajalle aseen. Aliohjelmaa käytetään, jotta toistoa saadaan vähennettyä.
    /// Pelaaja voi vaihtaa asetta kahden erilaisen aseen välillä.
    /// </summary>
    /// <param name="tulitusnopeus">MOntako ammusta voidaan ampua sekunnissa.</param>
    /// <param name="vahinko">Aseen aiheuttama vahinko.</param>
    /// <param name="kuva">Kuva, joka halutaan antaa aseelle.</param>
    private void LuoAse(int tulitusnopeus, int vahinko, Image kuva)
    {
        pelaaja1Ase = new AssaultRifle(25, 25);
        pelaaja1.Add(pelaaja1Ase);
        pelaaja1Ase.FireRate = tulitusnopeus;
        pelaaja1Ase.Position = pelaaja1.Position + new Vector(pelaaja1.Width / 2.5, 0);
        pelaaja1Ase.MaxAmmoLifetime = TimeSpan.FromSeconds(3);
        pelaaja1Ase.Image = kuva;
        asevahinko = vahinko;
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
        pomo.Color = Color.SlateGray;
        pomo.Tag = "pomo";
        Add(pomo);

        //Aivojen tekeminen, jolla saadan pomo liikkumaan kentällä.
        PlatformWandererBrain pomoaivot = new PlatformWandererBrain();
        pomoaivot.FallsOffPlatforms = true;
        pomoaivot.TriesToJump = true;
        pomoaivot.JumpSpeed = 100;
        pomoaivot.Speed = 50;
        pomo.Brain = pomoaivot;

        pomoAse = LuoPomonAse(2, 0);
        pomoAse2 = LuoPomonAse(-2, 0);
        pomoAse2.Angle = Angle.FromDegrees(180);
        
        pomoLaskuri= new IntMeter(PomoTerveys, 0, PomoTerveys);
        pomoLaskuri.LowerLimit += PomoKuoli; //Kun laskuri saa pienimmän arvon eli 0, pomo kuolee.
        pomoLaskuri.AddTrigger(25, TriggerDirection.Down, Tuhoavihut);
        
        ProgressBar pomopalkki = new ProgressBar(500, 40, pomoLaskuri);
        pomopalkki.X = 0;
        pomopalkki.Y = Screen.Top - 25;
        pomopalkki.BarColor = Color.Red;
        pomopalkki.Color = Color.Black;
        pomopalkki.BorderColor = Color.Black;
        pomopalkki.Angle = Angle.FromDegrees(180);
        Add(pomopalkki);
    }


    /// <summary>
    /// Luo pomonaseet erillisellä aliohjelmalla, jotta saadaan toistoa pois.
    /// Voidaan myös lisätä pomolle lisää aseita, jos on tarvetta, helposti.
    /// </summary>
    /// <param name="sijaintix">Paikka x-akselilla, johon ase tulee suhteutettuna pomon sijaintiin.</param>
    /// /// <param name="sijaintiy">Paikka y-akselilla, johon ase tulee suhteutettuna pomon sijaintiin.</param>
    private Cannon LuoPomonAse(double sijaintix, double sijaintiy)
    {
        Cannon ase = new Cannon(30, 20);
        pomo.Add(ase);
        ase.Image = pomoAseKuva;
        ase.FireRate = 0.4;
        ase.Position = pomo.Position + new Vector(pomo.Width / sijaintix, sijaintiy);
        ase.MaxAmmoLifetime = TimeSpan.FromSeconds(2.5);
        ase.CanHitOwner = true;
        return ase;
    }
    
    private void Tuhoavihut()
    {
        int vihujenmaara = vihut.Count;
        terveyslaskuri.Value = 10;
        MessageDisplay.Add("Parannuit täysin!");

        if (vihujenmaara > 0)
        {
            for (int i = 0; i < vihujenmaara; i++)
            {
                vihut[i].Destroy();
            }
        }
    }
    

    /// <summary>
    /// Näppäimet tehdään käyttäen Keyboard.Listen -toimintoa. Kun painaa tiettyä nappia siirrytään
    /// haluttuun aliohjelmaan oikeilla parametreilla, jos niitä tarvitaan.
    /// </summary>
    private void LisaaNappaimet()
    {
        //Liikkuminen:
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -Liikenopeus);
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "Liikkuu oikealle", pelaaja1, Liikenopeus);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, Hyppynopeus);
        Keyboard.Listen(Key.Space, ButtonState.Down, Hyppaa, "Pelaaja hyppää", pelaaja1, Hyppynopeus);
        
        //Muut näppäimet:
        Keyboard.Listen(Key.D1, ButtonState.Pressed, VaihdaAse, "Vaihtaa aseen");
        Keyboard.Listen(Key.R, ButtonState.Down, AloitaAlusta, "Aloittaa pelin alusta"); //Ei tarvitse ajaa peliä uudelleen, jos haluaa aloittaa alusta
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


    /// <summary>
    /// Vaihtaa pelaajan aseen, jos on kerätty uusia aseita. 
    /// </summary>
    private void VaihdaAse()
    {
        if (asekeratty)
        {
            if (asevahinko == 1)
            {
                pelaaja1Ase.Destroy();
                LuoAse(1, 5, aseKuva2);
                asevahinko = 5;
                MessageDisplay.Add("Ase vaihdettu! Vahinko on nyt:" + asevahinko);
            }

            else
            {
                pelaaja1Ase.Destroy();
                LuoAse(3, 1, aseKuva1);
                asevahinko = 1;
                MessageDisplay.Add("Ase vaihdettu! Vahinko on nyt:" + asevahinko);
            }
        }
        
        else MessageDisplay.Add("Ei asetta vaihdettavaksi!");
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


    /// <summary>
    /// Aliohjelmassa katsotaan, minkä asian pelaaja keräsi if-lauseen avulla.
    /// Pelaajalla ja kerättävälle asialle tehdään halutut muutokset sen mukaan, mitä kerättiin.
    /// </summary>
    /// <param name="hahmo">pelaaja, joka keräsi tavaraa</param>
    /// <param name="kerattava">esine, johon pelaaja osui ja keräsi sen</param>
    private void Keraa(PhysicsObject hahmo, PhysicsObject kerattava)
    {
        maaliAani.Play();
        MessageDisplay.Add("Keräsit " + kerattava.Tag+"!");
        kerattava.Destroy();

        if (kerattava.Tag.ToString() == "kristalli")
        {
            pistelaskuri.AddOverTime(Kristalliarvo * kerroin, 0.5);
            kristallit.Remove(kerattava);
        }
        
        if (kerattava.Tag.ToString() == "sydan")
        {
            terveyslaskuri.AddOverTime(1, 0.5);
            pistelaskuri.AddOverTime(Sydanarvo * kerroin, 0.5);
        }
        
        if (kerattava.Tag.ToString() == "ase1")
        {
            pistelaskuri.AddOverTime(1000 * kerroin, 0.5);
            asekeratty = true;
            MessageDisplay.Add("Paina 1 vaihtaaksesi asetta!");
        }
    }
    
    
    private void Osui(PhysicsObject hahmo, PhysicsObject osuttu)
    {
        MessageDisplay.Add("Autss...");
        terveyslaskuri.AddValue(-1);
    }
    
    
    private void OsuiKuolettavasti(PhysicsObject hahmo, PhysicsObject tappava)
    {
        MessageDisplay.Add("Kuolit!");
        terveyslaskuri.Value = 0;
    }
    
    
    private void PelaajaKuoli()
    {
        pelaaja1.Destroy();
        topLista.HighScoreWindow.NameInputWindow.Message.Text = "Kuolit, oppiipahan olemaan! Sait {0:0.00} pistettä!";
        topLista.Text = "Yritähän uudelleen!";
        topLista.EnterAndShow(pistelaskuri.Value);
        topLista.HighScoreWindow.Closed += AloitaPeli;
    }
    

    private void PomoKuoli()
    {
        pomo.Destroy();
        pistelaskuri.AddOverTime(kerroin*10000, 10);
        VoititPelin();
    }
    
    
    /// <summary>
    /// TArkistetaan, mihin ammus osui ja suoritetaan halutut komennot sen mukaan.
    /// </summary>
    /// <param name="ammus">pelaajan aseen tuottama ammus</param>
    /// <param name="kohde">kohde, johon ammus osui, jonka mukaan suoritetaan halutut toiminnot</param>
    private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        MessageDisplay.Add("Ammus osui: " + kohde.Tag);

        //Varmistetaan, että osuttu kohde on oikea ja saadaan tehtyä oikeat ohjelmat
        if (kohde.Tag.ToString() == "vihollinen")
        {
            kohde.Destroy();
            Muutavihut();
            pistelaskuri.AddOverTime(500 * kerroin * asevahinko, 0.5);
            ammus.Destroy();
        }

        //Varmistetaan, että osuttu kohde on oikea ja saadaan tehtyä oikeat ohjelmat
        if (kohde.Tag.ToString() == "pomo")
        {
            PomoAmpuu();
            pomoLaskuri.AddValue(-1*asevahinko);
            pistelaskuri.AddOverTime(100 * kerroin * asevahinko, 0.5);
            ammus.Destroy();
            Muutavihut();
        }

        if (kohde.Tag.ToString() == "taso" || kohde.Tag.ToString() == "kuolettava")
        {
            ammus.Destroy();
        }
    }
    
    
    /// <summary>
    /// Laskee pisteet kaikista kerätyistä asioista ja peliajasta, kun pomo on tapettu.
    /// </summary>
    private void VoititPelin()
    {
        double pelattuAika = aikalaskuri.SecondCounter.Value;
        double jaanytTerveys = terveyslaskuri.Value * 10;
        double peliAika = 1000 / pelattuAika; //Mitä nopeammin pelattu, sitä enemmän saadaan pisteitä
        pistelaskuri.MultiplyValue(peliAika);
        pistelaskuri.MultiplyValue(jaanytTerveys);
        pelaaja1.Destroy();
        topLista.HighScoreWindow.NameInputWindow.Message.Text = "Onneksi olkoon, voitit pelin! Sait {0:0.00} pistettä!";
        topLista.Text = "Voitit pelin!";
        topLista.EnterAndShow(pistelaskuri.Value);
        topLista.HighScoreWindow.Closed += AloitaPeli;
    }
}
}

