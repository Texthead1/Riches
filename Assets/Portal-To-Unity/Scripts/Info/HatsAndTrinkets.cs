using System.ComponentModel;

namespace PortalToUnity
{
    public enum HatType
	{
		[Description("No Hat")]
		None = 0,
		[Description("Combat Hat")]
		Combat = 1,
		[Description("Napoleon Hat")]
		Napoleon = 2,
		[Description("Spy Gear")]
		SpyGear = 3,
		[Description("Miner Hat")]
		Miner = 4,
		[Description("General's Hat")]
		GeneralsHat = 5,
		[Description("Pirate Hat")]
		Pirate = 6,
		[Description("Propeller Cap")]
		Propeller = 7,
		[Description("Coonskin Cap")]
		Coonskin = 8,
		[Description("Straw Hat")]
		Straw = 9,
		[Description("Fancy Hat")]
		Fancy = 10,
		[Description("Top Hat")]
		TopHat = 11,
		[Description("Viking Helmet")]
		VikingHelmet = 12,
		[Description("Spiked Hat")]
		Spiked = 13,
		[Description("Anvil Hat")]
		Anvil = 14,
		[Description("Beret")]
		Beret = 15,
		[Description("Birthday Hat")]
		Birthday = 16,
		[Description("Bone Head")]
		BoneHead = 17,
		[Description("Bowler Hat")]
		Bowler = 18,
		[Description("Wabbit Ears")]
		WabbitEars = 19,
		[Description("Tropical Turban")]
		TropicalTurban = 20,
		[Description("Chef Hat")]
		Chef = 21,
		[Description("Cowboy Hat")]
		Cowboy = 22,
		[Description("Rocker Hair")]
		RockerHair = 23,
		[Description("Royal Crown")]
		RoyalCrown = 24,
		[Description("Lil Devil")]
		LilDevil = 25,
		[Description("Eye Hat")]
		Eye = 26,
		[Description("Fez")]
		Fez = 27,
		[Description("Crown of Light")]
		CrownOfLight = 28,
		[Description("Jester Hat")]
		Jester = 29,
		[Description("Winged Hat")]
		Winged = 30,
		[Description("Moose Hat")]
		Moose = 31,
		[Description("Plunger Head")]
		PlungerHead = 32,
		[Description("Pan Hat")]
		Pan = 33,
		[Description("Rocket Hat")]
		Rocket = 34,
		[Description("Santa Hat")]
		Santa = 35,
		[Description("Tiki Hat")]
		Tiki = 36,
		[Description("Trojan Helmet")]
		TrojanHelmet = 37,
		[Description("Unicorn Hat")]
		Unicorn = 38,
		[Description("Wizard Hat")]
		Wizard = 39,
		[Description("Pumpkin Hat")]
		Pumpkin = 40,
		[Description("Pirate Doo Rag")]
		PirateDooRag = 41,
		[Description("Cossack Hat")]
		Cossack = 42,
		[Description("Flower Hat")]
		Flower = 43,
		[Description("Balloon Hat")]
		Ballon = 44,
		[Description("Happy Birthday!")]
		HappyBirthday = 45,
		[Description("Vintage Baseball Cap")]
		VintageBaseball = 46,
		[Description("Bowling Pin Hat")]
		BowlingPin = 48,
		[Description("Officer Cap")]
		Officer = 49,
		[Description("Firefighter Helmet")]
		Firefighter = 50,
		[Description("Graduation Hat")]
		Graduation = 51,
		[Description("Lampshade Hat")]
		Lampshade = 52,
		[Description("Mariachi Hat")]
		Mariachi = 53,
		[Description("Paper Fast Food Hat")]
		PaperFastFood = 55,
		[Description("Pilgrim Hat")]
		Pilgrim = 56,
		[Description("Police Siren Hat")]
		PoliceSiren = 57,
		[Description("Purple Fedora")]
		PurpleFedora = 58,
		[Description("Archer Hat")]
		Archer = 59,
		[Description("Safari Hat")]
		Safari = 61,
		[Description("Sailor Hat")]
		Sailor = 62,
		[Description("Dancer Hat")]
		Dancer = 64,
		[Description("Traffic Cone Hat")]
		TrafficCone = 65,
		[Description("Turban")]
		Turban = 66,
		[Description("Battle Helmet")]
		BattleHelmet = 67,
		[Description("Bottle Cap Hat")]
		BottleCap = 68,
		[Description("Carrot Hat")]
		Carrot = 70,
		[Description("Elf Hat")]
		Elf = 72,
		[Description("Fishing Hat")]
		Fishing = 73,
		[Description("Future Hat")]
		Future = 74,
		[Description("Nefertiti")]
		Nefertiti = 75,
		[Description("Pants Hat")]
		Pants = 77,
		[Description("Princess Hat")]
		Princess = 78,
		[Description("Tiy Soldier Hat")]
		ToySoldier = 79,
		[Description("Trucker Hat")]
		Trucker = 80,
		[Description("Umbrella Hat")]
		Umbrella = 81,
		[Description("Showtime Hat")]
		Showtime = 82,
		[Description("Caesar Hat")]
		Caesar = 83,
		[Description("Flower Fairy Hat")]
		FlowerFairy = 84,
		[Description("Funnel Hat")]
		Funnel = 85,
		[Description("Scrumshanks Hat")]
		Scrumshanks = 86,
		[Description("Biter Hat")]
		Biter = 87,
		[Description("Atom Hat")]
		Atom = 88,
		[Description("Sombrero")]
		Sombrero = 89,
		[Description("Rasta Hat")]
		Rasta = 90,
		[Description("Kufi Hat")]
		Kufi = 91,
		[Description("Knight Helm")]
		KnightHelm = 92,
		[Description("Dangling Carrot Hat")]
		DanglingCarrot = 93,
		[Description("Bronze Top Hat")]
		BronzeTopHat = 94,
		[Description("Silver Top Hat")]
		SilverTopHat = 95,
		[Description("Gold Top Hat")]
		GoldTopHat = 96,
		[Description("Rain Hat")]
		Rain = 97,
		[Description("The Outsider")]
		TheOutsider = 98,
		[Description("Greeble Hat")]
		Greeble = 99,
		[Description("Volcano Hat")]
		Volcano = 100,
		[Description("Boater Hat")]
		Boater = 101,
		[Description("Stone Hat")]
		Stone = 102,
		[Description("Stovepipe Hat")]
		Stovepipe = 103,
		[Description("Boonie Hat")]
		Boonie = 104,
		[Description("Sawblade Hat")]
		Sawblade = 105,
		[Description("Zomeanie Hat")]
		Zombeanie = 106,
		[Description("Gaucho Hat")]
		Gaucho = 107,
		[Description("Roundlet Hat")]
		Roundlet = 108,
		[Description("Capuchon")]
		Capuchon = 109,
		[Description("Tricorn Hat")]
		Tricorn = 110,
		[Description("Feathered Headdress")]
		FeatheredHeaddress = 111,
		[Description("Bearskin Cap")]
		Bearskin = 112,
		[Description("Fishbone Hat")]
		Fishbone = 113,
		[Description("Ski Cap")]
		SkiCap = 114,
		[Description("Crown of Frost")]
		CrownOfFrost = 115,
		[Description("Four Winds")]
		FourWinds = 116,
		[Description("Beacon Hat")]
		Beacon = 117,
		[Description("Flower Garland")]
		FlowerGarland = 118,
		[Description("Tree Branch")]
		TreeBranch = 119,
		[Description("Aviator's Cap")]
		AviatorsCap = 120,
		[Description("Asteroid Hat")]
		Asteroid = 121,
		[Description("Crystal Hat")]
		Crystal = 122,
		[Description("Creepy Helm")]
		CreepyHelm = 123,
		[Description("Fancy Ribbon")]
		FancyRibbon = 124,
		[Description("Deely Boppers")]
		DeelyBoppers = 125,
		[Description("Beanie")]
		Beanie = 126,
		[Description("Leprechaun Hat")]
		Leprechaun = 127,
		[Description("Shark Hat")]
		Shark = 128,
		[Description("Life Preserver Hat")]
		LifePreserver = 129,
		[Description("Glittering Tiara")]
		GlitteringTiara = 130,
		[Description("Great Helm")]
		GreatHelm = 131,
		[Description("Space Helmet")]
		SpaceHelmet = 132,
		[Description("UFO Hat")]
		UFOHat = 133,
		[Description("Whirlwind Diadem")]
		WhirlwindDiadem = 134,
		[Description("Obsidian Helm")]
		ObsidianHelm = 135,
		[Description("Lilypad Hat")]
		Lilypad = 136,
		[Description("Crown of Flames")]
		CrownOfFlames = 137,
		[Description("Runic Headband")]
		RunicHeadband = 138,
		[Description("Clockwork Hat")]
		Clockwork = 139,
		[Description("Cactus Hat")]
		Cactus = 140,
		[Description("Skullhelm")]
		Skullhelm = 141,
		[Description("Gloop Hat")]
		Gloop = 142,
		[Description("Puma Hat")]
		Puma = 143,
		[Description("Elephant Hat")]
		Elephant = 144,
		[Description("Tiger Skin Cap")]
		TigerSkin = 145,
		[Description("Teeth Top Hat")]
		TeethTop = 146,
		[Description("Turkey Hat")]
		Turkey = 147,
		[Description("Eyefro")]
		Eyefro = 148,
		[Description("Bacon Bandana")]
		BaconBandana = 149,
		[Description("Awesome Hat")]
		Awesome = 150,
		[Description("Card Shark Hat")]
		CardShark = 151,
		[Description("Springtime Hat")]
		Springtime = 152,
		[Description("Jolly Hat")]
		Jolly = 153,
		[Description("Kickoff Hat")]
		Kickoff = 154,
		[Description("Beetle Hat")]
		Beetle = 155,
		[Description("Brain Hat")]
		Brain = 156,
		[Description("Brainiac Hat")]
		Brainiac = 157,
		[Description("Bucket Hat")]
		Bucket = 158,
		[Description("Desert Crown")]
		DesertCrown = 159,
		[Description("Ceiling Fan Hat")]
		CeilingFan = 160,
		[Description("Imperial Hat")]
		Imperial = 161,
		[Description("Clown Classic Hat")]
		ClownClassic = 162,
		[Description("Clown Bowler Hat")]
		ClownBowler = 163,
		[Description("Colander Hat")]
		Colander = 164,
		[Description("Kepi Hat")]
		Kepi = 165,
		[Description("Cornucopia Hat")]
		Cornucopia = 166,
		[Description("Cubano Hat")]
		Cubano = 167,
		[Description("Cycling Hat")]
		Cycling = 168,
		[Description("Daisy Crown")]
		DaisyCrown = 169,
		[Description("Dragon Skull")]
		DragonSkull = 170,
		[Description("Outback Hat")]
		Outback = 171,
		[Description("Lil' Elf Hat")]
		LilElf = 172,
		[Description("Generalissimo")]
		Generalissimo = 173,
		[Description("Garrison Hat")]
		Garrison = 174,
		[Description("Gondolier Hat")]
		Gondolier = 175,
		[Description("Hunting Hat")]
		Hunting = 176,
		[Description("Juicer Hat")]
		Juicer = 177,
		[Description("Kokoshnik")]
		Kokoshnik = 178,
		[Description("Medic Hat")]
		Medic = 179,
		[Description("Melon Hat")]
		Melon = 180,
		[Description("Mountie Hat")]
		Mountie = 181,
		[Description("Nurse Hat")]
		Nurse = 182,
		[Description("Palm Hat")]
		Palm = 183,
		[Description("Paperboy Hat")]
		Paperboy = 184,
		[Description("Parrot Nest")]
		ParrotNest = 185,
		[Description("Old-Time Movie Hat")]
		OldTimeMovie = 186,
		[Description("Classic Pot Hat")]
		ClassicPot = 187,
		[Description("Radar Hat")]
		Radar = 188,
		[Description("Crazy Light Bulb Hat")]
		CrazyLightBulb = 189,
		[Description("Rubber Glove")]
		RubberGlove = 190,
		[Description("Rugby Hat")]
		Rugby = 191,
		[Description("Sharkfin Hat")]
		Sharkfin = 192,
		[Description("Sleuth Hat")]
		Sleuth = 193,
		[Description("Shower Cap")]
		ShowerCap = 194,
		[Description("Bobby")]
		Bobby = 195,
		[Description("Hedgehog Hat")]
		Hedgehog = 196,
		[Description("Steampunk Hat")]
		Steampunk = 197,
		[Description("Flight Attendant Hat")]
		FlightAttendant = 198,
		[Description("Monday Hat")]
		Monday = 199,
		[Description("Sjerpa Hat")]
		Sherpa = 200,
		[Description("Trash Lid")]
		TrashLid = 201,
		[Description("Turtle Hat")]
		Turtle = 202,
		[Description("Extreme Viking Hat")]
		ExtremeViking = 203,
		[Description("Scooter Hat")]
		Scooter = 204,
		[Description("Volcano Island Hat")]
		VolcanoIsland = 205,
		[Description("Synchronized Swimming Cap")]
		SynchronizedSwimmingCap = 206,
		[Description("William Tell Hat")]
		WilliamTell = 207,
		[Description("Tribal Hat")]
		Tribal = 208,
		[Description("Rude Boy Hat")]
		RudeBoy = 209,
		[Description("Pork Pie Hat")]
		PorkPie = 210,
		[Description("Alarm Clock Hat")]
		AlarmClock = 211,
		[Description("Batter Up Hat")]
		BatterUp = 212,
		[Description("Horns Be With You")]
		HornsBeWithYou = 213,
		[Description("Croissant Hat")]
		Croissant = 214,
		[Description("Weather Vane Hat")]
		WeatherVane = 215,
		[Description("Rainbow Hat")]
		Rainbow = 216,
		[Description("Eye of Kaos Hat")]
		EyeOfKaos = 217,
		[Description("Bat Hat")]
		Bat = 218,
		[Description("Light Bulb")]
		LightBulb = 219,
		[Description("Firefly Jar")]
		FireflyJar = 220,
		[Description("Shadow Ghost Hat")]
		ShadowGhost = 221,
		[Description("Lighthouse Beacon Hat")]
		LighthouseBeacon = 222,
		[Description("Tin Foil Hat")]
		TinFoil = 223,
		[Description("Night Cap")]
		NightCap = 224,
		[Description("Storm Hat")]
		Storm = 225,
		[Description("Gold Arkeyan Helm")]
		GoldArkeyanHelm = 226,
		[Description("Toucan Hat")]
		Toucan = 227,
		[Description("Pyramid Hat")]
		Pyramid = 228,
		[Description("Miniature Skylands Hat")]
		MiniatureSkylands = 229,
		[Description("Sorcerer Hat")]
		Wizard_2 = 230,
		[Description("Candy Cane Hat")]
		CandyCane = 232,
		[Description("Eggshell Hat")]
		Eggshell = 233,
		[Description("Candle Hat")]
		Candle = 234,
		[Description("Dark Helm")]
		DarkHelm = 235,
		[Description("Planet Hat")]
		Planet = 236,
		[Description("Bellhop Hat")]
		Bellhop = 237,
		[Description("Bronze Arkeyan Helm")]
		BronzeArkeyanHelm = 238,
		[Description("Silver Arkeyan Helm")]
		SilverArkeyanHelm = 239,
		[Description("Raver Hat")]
		Raver = 240,
		[Description("Shire Hat")]
		Shire = 241,
		[Description("Mongol Hat")]
		Mongol = 242,
		[Description("Skipper Hat")]
		Skipper = 243,
		[Description("Medieval Bard Hat")]
		MedievalBard = 244,
		[Description("Wooden Hat")]
		Wooden = 245,
		[Description("Carnival Hat")]
		Carnival = 246,
		[Description("Coconut Hat")]
		Coconut = 247,
		[Description("Model Home Hat")]
		ModelHome = 248,
		[Description("Ice Cream Hat")]
		IceCream = 249,
		[Description("Molekin Mountain Hat")]
		MolekinMountain = 250,
		[Description("Sheepwrecked Hat")]
		Sheepwrecked = 251,
		[Description("Core of Light Hat")]
		CoreOfLight = 252,
		[Description("Octavius Cloptimus Hat")]
		OctaviusCloptimus = 253,
		[Description("Dive Bomber Hat")]
		DiveBomber = 260,
		[Description("Sea Shadow Hat")]
		SeaShadow = 261,
		[Description("Burn-Cycle Header")]
		BurnCycleHeader = 262,
		[Description("Reef Ripper Helmet")]
		ReefRipperHelmet = 263,
		[Description("Jet Stream Helmet")]
		JetStreamHelmet = 264,
		[Description("Soda Skimmer Shower Cap")]
		SodaSkimmerShowerCap = 265,
		[Description("Tomb Buggy Skullcap")]
		TombBuggySkullcap = 266,
		[Description("Stealth Stinger Beanie")]
		StealthStingerBeanie = 267,
		[Description("Skark Tank Topper")]
		SharkTankTopper = 268,
		[Description("Gold Rusher Cog")]
		GoldRusherCog = 269,
		[Description("Splatter Splasher Spires")]
		SplatterSplasherSpires = 270,
		[Description("Thump Trucker's Hat")]
		ThumpTruckersHat = 271,
		[Description("Buzz Wing Hat")]
		BuzzWing = 272,
		[Description("Shield Striker Helmet")]
		ShieldStrikerHelmet = 273,
		[Description("Sun Runner Spikes")]
		SunRunnerSpikes = 274,
		[Description("Hot Streak Headpiece")]
		HotStreakHeadpiece = 275,
		[Description("Sky Slicer Hat")]
		SkySlicer = 276,
		[Description("Crypt Crusher Cap")]
		CryptCrusher = 277,
		[Description("Mags Hat")]
		Mags = 278,
		[Description("Kaos Krown")]
		KaosKrown = 279,
		[Description("Eon's Helm")]
		EonsHelm = 280,
	}

	public enum TrinketType
	{
		None = 0,
		LuckyTie = 1,
		Bubble = 2,
		DarkWaterDaisy = 3,
		VoteForCyclops = 4,
		DragonHorn = 5,
		Iris = 6,
		KuckooKazoo = 7,
		Rune = 8,
		UllyssesUniclops = 9,
		BillyBison = 10,
		StealthElfGift = 11,
		LizardLilly = 12,
		PiratePinwheel = 13,
		BubbleBlower = 14,
		MedalOfHeroism = 15,
		MedalOfCourage = 16,
		MedalOfValiance = 17,
		MedalOfGallantry = 18,
		MedalOfMettle = 19,
		WingedMedalOfBravery = 20,
		SeadogSeashell = 21,
		Sunflower = 22,
		TeddyCyclops = 23,
		GooFactoryGear = 24,
		ElementalOpal = 25,
		ElementalRadiant = 26,
		ElementalDiamond = 27,
		CyclopsSpinner = 28,
		WilikinWindmill = 29,
		TimeTownTicker = 30,
		BigBowOfBoom = 31,
		MabusMedallion = 32,
		SpyrosShield = 33
	}
}