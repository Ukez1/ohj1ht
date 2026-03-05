using System;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;

/// @author gr313135
/// @version 18.11.2025
/// <summary>
/// 
/// </summary>

class Vihu : PlatformCharacter
{
    private IntMeter elamalaskuri = new IntMeter(3, 0, 3);
    public  IntMeter Elamalaskuri { get { return elamalaskuri; } }

    public Vihu(double leveys, double korkeus)
        : base(leveys, korkeus)
    {
        elamalaskuri.LowerLimit += delegate { this.Destroy(); };
    }
}
public class CrystalDungeon : PhysicsGame
{
    private const double NOPEUS = 200;
    private const double HYPPYNOPEUS = 650;
    private const int RUUDUN_KOKO = 40;
    private PlatformCharacter pelaaja1;
    private PlatformCharacter vihollinen;
    private PlatformCharacter pomo;
    private Image[] kristalliAnimaatio = LoadImages("Kristalli1", "Kristalli2", "Kristalli3", "Kristalli4", "Kristalli5", "Kristalli6", "Kristalli7", "Kristalli8");
    private Image pelaajan1Kuva = LoadImage("pelaaja.png");
    private Image tippukiviKuva = LoadImage("Tippukivi.png");
    private Image piikkiKuva = LoadImage("Piikki.png");
    private Image liikkuvaTasoKuva = LoadImage("Leijuvakivi.png");
    private Image tyhjaPomopalkki = LoadImage("tyhjapalkki");
    private Image taysiPomopalkki = LoadImage("taysipalkki");
    private SoundEffect maaliAani = LoadSoundEffect("maali.wav");
    
    private IntMeter pistelaskuri;
    private IntMeter terveyslaskuri;
    private IntMeter pomoLaskuri;
    private Timer aikalaskuri;
    private EasyHighScore topLista = new EasyHighScore();
    AssaultRifle pelaaja1Ase;
    Cannon pomoAse;
    private PhysicsObject tippukivi;
    int kerroin = 1;
    private int arpa;
    
    public override void Begin()
    {
        AloitaAlusta();
    }
    
    
    public void AloitaPeli(Window sender)
    {
        AloitaAlusta();
    }
    

    private void AloitaAlusta()
    {
        ClearAll(); // poistaa kaiken
        LuoKentta();
        LisaaNappaimet();
        LuoPistelaskuri();
        LuoTerveysLaskuri();
        LuoAikalaskuri();
        
        //MediaPlayer.Play("taustaMusiikki");
        //MediaPlayer.IsRepeating = true;
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1;
        Camera.StayInLevel = true;
        MasterVolume = 0.5;
        Gravity = new Vector(0, -1000);
        
        Timer.CreateAndStart(2, Paivita);
    }
    
    
    public static Double Itseisarvo(Double luku)
    {
        if (luku < 0)
        {
            return -luku;
        }
    
        else return luku;
    }
    

    public static Double Etaisyys(double a, double b)
    {
        return Itseisarvo((a-b));
    }
    

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
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        LisaaHuonotKristallit(20, 50);
        Level.CreateBorders();
        //Level.Background.CreateGradient(Color.AshGray, Color.Black);
        Level.BackgroundColor = Color.White;

    }
    

    private void Paivita()
    {
        
    }


    private void LisaaVihollinen(Vector paikka, double leveys, double korkeus)
    {
        vihollinen = new PlatformCharacter(leveys, korkeus);
        vihollinen.Position = paikka;
        vihollinen.Mass = 5.0;
        vihollinen.Shape = Shape.Circle;
        vihollinen.Color = Color.Green;
        vihollinen.Tag = "vihollinen";
        Add(vihollinen);

        PlatformWandererBrain tasoaivot = new PlatformWandererBrain(); //Saadaan vihu liikkumaan kentällä
        tasoaivot.Speed = 70;

        vihollinen.Brain = tasoaivot;

    }
    
    
    private void LisaaHuonotKristallit(int montako, double koko)
    {
        PhysicsObject[] huonotKristallit = new PhysicsObject[montako];
        for (int i = 0; i < montako; i++)
        {
            PhysicsObject huonoKristalli = new PhysicsObject(koko, koko);
            huonotKristallit[i] = huonoKristalli;
            huonoKristalli.IgnoresCollisionResponse = true;
            huonoKristalli.MakeStatic();
            huonoKristalli.Color = Color.BloodRed;
            huonoKristalli.Position = Level.GetRandomFreePosition(30);
            Add(huonoKristalli);
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
        PhysicsObject taso2 = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso2.Position = paikka;
        taso2.Color = Color.DarkGray;
        Add(taso2);
    }
    
    
    private void LisaaPiikki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject piikki = PhysicsObject.CreateStaticObject(leveys, korkeus);
        piikki.Position = paikka;
        piikki.Shape = Shape.Triangle;
        piikki.Color = Color.HotPink;
        piikki.Image = piikkiKuva;
        piikki.Tag = "piikki";
        Add(piikki);
    }
    
    
    private void LisaaIsoPiikki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject isoPiikki = PhysicsObject.CreateStaticObject(leveys, korkeus);
        isoPiikki.Position = paikka;
        isoPiikki.Shape = Shape.Triangle;
        isoPiikki.Color = Color.BloodRed;
        isoPiikki.Tag = "tappava";
        Add(isoPiikki);
    }
    
    
    private void LisaaTippukivi(Vector paikka, double leveys, double korkeus)
    {
        tippukivi = new PhysicsObject(leveys, korkeus);
        tippukivi.Position = paikka;
        tippukivi.Shape = Shape.Triangle;
        tippukivi.Color = Color.Wheat;
        tippukivi.Angle = Angle.FromDegrees(180);
        tippukivi.IgnoresGravity = true;
        tippukivi.Tag = "tappava";
        tippukivi.CanRotate = false;
        tippukivi.Image = tippukiviKuva;
        Add(tippukivi);
        
        //Työn alla. Tavoite saada tippukivi tippumaan, kun pelaaja on sen alla.
        PhysicsObject trigger = PhysicsObject.CreateStaticObject(leveys, 200);
        trigger.Position = paikka - new Vector(0, 100);
        trigger.IsVisible = false;
        trigger.Tag = "tippukiviTrigger";
        Add(trigger);
        
        //AddCollisionHandler(pelaaja1, trigger, (p, t) =>
        //{
        //    t.IgnoresGravity = false;
        //    trigger.Destroy();
        //});
    }
    

    private void LisaaKristalli(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kristalli = PhysicsObject.CreateStaticObject(leveys, korkeus);
        kristalli.IgnoresCollisionResponse = true;
        kristalli.Position = paikka;
        kristalli.Animation = new Animation(kristalliAnimaatio);
        kristalli.Animation.Start();
        kristalli.Animation.FPS = 8;
        kristalli.Tag = "kristalli";
        Add(kristalli);
        
        arpa = RandomGen.SelectOne(1, 2, 3, 4, 5);
    }
    
    
    private void LisaaSydan(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject sydan = PhysicsObject.CreateStaticObject(leveys, korkeus);
        sydan.IgnoresCollisionResponse = true;
        sydan.Position = paikka;
        sydan.Shape = Shape.Heart;
        sydan.Color = Color.Red;
        sydan.Tag = "sydan";
        Add(sydan);
    }
    
    
    private void LisaaLiikkuvaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject liikkuvataso = PhysicsObject.CreateStaticObject(leveys, korkeus/2);
        liikkuvataso.Position = paikka;
        liikkuvataso.Color = Color.Charcoal;
        liikkuvataso.IgnoresGravity = true;
        liikkuvataso.Oscillate(Vector.UnitY, 150, 0.2); //Tekee liikkeen
        liikkuvataso.Image = liikkuvaTasoKuva;
        Add(liikkuvataso);
    }
    
    
    private void LisaaMurskain(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject murskain = PhysicsObject.CreateStaticObject(leveys, korkeus);
        murskain.Position = paikka;
        murskain.Color = Color.Black;
        murskain.IgnoresGravity = true;
        murskain.Oscillate(Vector.UnitY, 160, 0.1); //Tekee liikkeen
        murskain.Tag = "tappava";
        Add(murskain);
    }
    

    public void LuoPistelaskuri()
    {
        pistelaskuri = new IntMeter(0);               

        Label pistetaulukko = new Label(); 
        pistetaulukko.X = Screen.Right - 100;
        pistetaulukko.Y = Screen.Top - 50;
        pistetaulukko.TextColor = Color.Black;
        pistetaulukko.Color = Color.White;
        pistetaulukko.Title = "Pisteet: ";
        pistetaulukko.BindTo(pistelaskuri);
        Add(pistetaulukko);
    }
    
    
    public void LuoTerveysLaskuri()
    {
        terveyslaskuri= new IntMeter(5, 0, 10);
        terveyslaskuri.LowerLimit += PelaajaKuoli; //Kun laskuri saa pienimmän arvon eli 0, pelaaja kuolee.
        
        Label pelaajanTerveys = new Label(); 
        pelaajanTerveys.X = Screen.Right - 100;
        pelaajanTerveys.Y = Screen.Top - 70;
        pelaajanTerveys.TextColor = Color.Black;
        pelaajanTerveys.Color = Color.White;
        pelaajanTerveys.Title = "Terveys: ";
        pelaajanTerveys.BindTo(terveyslaskuri);
        Add(pelaajanTerveys);
        
        ProgressBar elamapalkki = new ProgressBar(150, 20);
        elamapalkki.X = Screen.Right - 100;
        elamapalkki.Y = Screen.Top - 70;
        elamapalkki.BarColor = Color.Red;
        elamapalkki.Color = Color.Transparent;
        elamapalkki.BorderColor = Color.Black;
        elamapalkki.BindTo(terveyslaskuri);
        Add(elamapalkki);
    }
    
    
    public void LuoAikalaskuri()
    {
        aikalaskuri = new Timer();
        aikalaskuri.Start();

        Label aikanaytto = new Label();
        aikanaytto.X = Screen.Right - 100;
        aikanaytto.Y = Screen.Top - 30;
        aikanaytto.TextColor = Color.Black;
        aikanaytto.Color = Color.White;
        aikanaytto.DecimalPlaces = 1;
        aikanaytto.BindTo(aikalaskuri.SecondCounter);
        Add(aikanaytto);
    }
    
    
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajan1Kuva;
        pelaaja1.IgnoresCollisionResponse = false;
        
        pelaaja1Ase = new AssaultRifle(30, 10);
        pelaaja1.Add(pelaaja1Ase);
        pelaaja1Ase.FireRate = 3;
        //pelaaja1Ase.ProjectileCollision = AmmusOsui;
        //pelaaja1Ase.ProjectileCollision = Ammusosuipomoon;
        pelaaja1Ase.Position = pelaaja1.Position + new Vector(pelaaja1.Width / 2, 0);
        pelaaja1Ase.MaxAmmoLifetime = TimeSpan.FromSeconds(3);
        
        AddCollisionHandler(pelaaja1, "kristalli", KeraaKristalli);
        AddCollisionHandler(pelaaja1, "piikki", Osui);
        AddCollisionHandler(pelaaja1, "sydan", KeraaSydan);
        AddCollisionHandler(pelaaja1, "tappava", OsuiKuolettavasti);
        AddCollisionHandler(pelaaja1, "vihollinen", Osui);
        AddCollisionHandler(pelaaja1, "pomo", Osui);
        Add(pelaaja1);
    }


    private void LisaaPomo(Vector paikka, double leveys, double korkeus)
    {
        pomo = new PlatformCharacter(leveys*2, korkeus*2);
        pomo.Position = paikka;
        pomo.Mass = 5.0;
        pomo.Shape = Shape.Circle;
        pomo.Color = Color.Green;
        pomo.Tag = "pomo";
        Add(pomo);

        PlatformWandererBrain pomoaivot = new PlatformWandererBrain(); //Saadaan pomo liikkumaan kentällä
        pomoaivot.FallsOffPlatforms = true;
        pomoaivot.TriesToJump = true;
        pomoaivot.JumpSpeed = 100;
        pomoaivot.Speed = 50;

        pomo.Brain = pomoaivot;
        
        pomoAse = new Cannon(30, 10);
        pomo.Add(pomoAse);
        pomoAse.FireRate = 1;
        //pelaaja1Ase.ProjectileCollision = AmmusOsui;
        //pelaaja1Ase.ProjectileCollision = Ammusosuipomoon;
        pomoAse.Position = pomo.Position + new Vector(pomo.Width / 2, 0);
        pomoAse.MaxAmmoLifetime = TimeSpan.FromSeconds(3);
        pomoAse.CanHitOwner = true;
        
        PhysicsObject kuula = pomoAse.Shoot();
        if (kuula != null) //Peli saattoi kaatua, jos ei tehnyt tarkistusta ja collisionhandler aktivoitui.
        {
            kuula.Tag = "Kuula";
            AddCollisionHandler<PhysicsObject, PlatformCharacter>(kuula, pelaaja1, Osui);
        }
        
        pomoLaskuri= new IntMeter(200, 0, 200);
        pomoLaskuri.LowerLimit += PomoKuoli; //Kun laskuri saa pienimmän arvon eli 0, pomo kuolee.
        
        ProgressBar pomopalkki = new ProgressBar(500, 60);
        pomopalkki.X = 0;
        pomopalkki.Y = Screen.Top - 25;
        pomopalkki.BarImage = tyhjaPomopalkki;
        pomopalkki.Image = taysiPomopalkki;
        pomopalkki.BorderColor = Color.Black;
        pomopalkki.BindTo(pomoLaskuri);
        Add(pomopalkki);
    }
    

    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        
        //Liikkuminen:
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
        Keyboard.Listen(Key.S, ButtonState.Pressed, Hyppaa, "", pelaaja1, -HYPPYNOPEUS);
        Keyboard.Listen(Key.Space, ButtonState.Down, Hyppaa, "", pelaaja1, HYPPYNOPEUS);
        
        //Muut näppäimet:
        Keyboard.Listen(Key.R, ButtonState.Down, AloitaAlusta, "Aloittaa pelin alusta");
        Keyboard.Listen(Key.LeftShift, ButtonState.Pressed, AktivoiHuijaukset, "");
        Mouse.Listen(MouseButton.Left, ButtonState.Down, Ammu, "Kutsuu loitsun");
    }
    

    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
        pelaaja1Ase.Angle = pelaaja1.Velocity.Angle;
    }
    

    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
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
        }
    }
    

    private void KeraaKristalli(PhysicsObject hahmo, PhysicsObject kristalli)
    {
        maaliAani.Play();
        MessageDisplay.Add("Keräsit kristallin!");
        pistelaskuri.AddOverTime(500*kerroin, 0.5);
        kristalli.Destroy();
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
        double PelattuAika = aikalaskuri.SecondCounter.Value;
        double PeliAika = 1000 / PelattuAika; //Mitä nopeammin pelattu, sitä enemmän saadaan pisteitä
        pistelaskuri.MultiplyValue(PeliAika);
        
        pelaaja1.Destroy();
        topLista.EnterAndShow(pistelaskuri.Value);
        topLista.HighScoreWindow.Closed += AloitaPeli;
        
    }

    private void PomoKuoli()
    {
        pomo.Destroy();
        pistelaskuri.AddOverTime(kerroin*10000, 10);
    }
    
    
    private void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        MessageDisplay.Add("Ammus osui: " + kohde.Tag);

        if (kohde.Tag == "vihollinen")
        {
            kohde.Destroy();
            pistelaskuri.AddOverTime(500 * kerroin, 0.5);
            ammus.Destroy();
        }

        if (kohde.Tag == "pomo")
        {
            pomoLaskuri.AddValue(-1);
            pistelaskuri.AddOverTime(100 * kerroin, 0.5);
            ammus.Destroy();
        }
    }
    

    private void AktivoiHuijaukset()
    {
        bool huijaukset = true;
        if (huijaukset)
        {
            kerroin = 1000;
            pelaaja1.IgnoresGravity = true;
        }
    }
    
    
}