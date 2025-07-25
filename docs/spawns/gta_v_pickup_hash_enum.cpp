// ref: https://pastebin.com/8EuSv2r1

enum ePickupHash : Hash
{
	PICKUP_AMMO_BULLET_MP = 0x2E4C762D,
	PICKUP_AMMO_FIREWORK = 0x10A73D59,
	PICKUP_AMMO_FIREWORK_MP = 0xD65BF49E,
	PICKUP_AMMO_FLAREGUN = 0x60F784C2,
	PICKUP_AMMO_GRENADELAUNCHER = 0x7E51DB8F,
	PICKUP_AMMO_GRENADELAUNCHER_MP = 0x7EBB7BCB,
	PICKUP_AMMO_HOMINGLAUNCHER = 0x16A73E3A,
	PICKUP_AMMO_MG = 0xAF272C6C,
	PICKUP_AMMO_MINIGUN = 0x5D95B557,
	PICKUP_AMMO_MISSILE_MP = 0xCA648B4F,
	PICKUP_AMMO_PISTOL = 0x43AAEAE6,
	PICKUP_AMMO_RIFLE = 0xE5EB8146,
	PICKUP_AMMO_RPG = 0x6F38E9FB,
	PICKUP_AMMO_SHOTGUN = 0x2D5CE030,
	PICKUP_AMMO_SMG = 0xFD4AE5E5,
	PICKUP_AMMO_SNIPER = 0x2451A293,
	PICKUP_ARMOUR_STANDARD = 0x38F01FB3,
	PICKUP_CAMERA = 0x6D4712EC,
	PICKUP_CUSTOM_SCRIPT = 0xE61E0AEB,
	PICKUP_GANG_ATTACK_MONEY = 0x67DAC98F,
	PICKUP_HEALTH_SNACK = 0xE85B5534,
	PICKUP_HEALTH_STANDARD = 0xC1F1FB04,
	PICKUP_MONEY_CASE = 0x77DA37A6,
	PICKUP_MONEY_DEP_BAG = 0x6A7A1932,
	PICKUP_MONEY_MED_BAG = 0xD9575FE9,
	PICKUP_MONEY_PAPER_BAG = 0x4323B0E8,
	PICKUP_MONEY_PURSE = 0x9FE15D36,
	PICKUP_MONEY_SECURITY_CASE = 0x30C774E1,
	PICKUP_MONEY_VARIABLE = 0xEA888D49,
	PICKUP_MONEY_WALLET = 0x0D8A2D82,
	PICKUP_PARACHUTE = 0xA1D4544E,
	PICKUP_PORTABLE_CRATE_FIXED_INCAR = 0x5B3683D3,
	PICKUP_PORTABLE_CRATE_FIXED_INCAR_SMALL = 0xBBA128BB,
	PICKUP_PORTABLE_CRATE_FIXED_INCAR_WITH_PASSENGERS = 0x9A8A755A,
	PICKUP_PORTABLE_CRATE_UNFIXED = 0x300100DF,
	PICKUP_PORTABLE_CRATE_UNFIXED_INAIRVEHICLE_WITH_PASSENGERS = 0xC4B30B34,
	PICKUP_PORTABLE_CRATE_UNFIXED_INCAR = 0x249F3ACE,
	PICKUP_PORTABLE_CRATE_UNFIXED_INCAR_SMALL = 0x65213263,
	PICKUP_PORTABLE_CRATE_UNFIXED_INCAR_WITH_PASSENGERS = 0x9A59B148,
	PICKUP_PORTABLE_CRATE_UNFIXED_LOW_GLOW = 0x49EB1321,
	PICKUP_PORTABLE_DLC_VEHICLE_PACKAGE = 0x72EA77D8,
	PICKUP_PORTABLE_PACKAGE = 0xCB6AE694,
	PICKUP_PORTABLE_PACKAGE_LARGE_RADIUS = 0x80E6F60B,
	PICKUP_SUBMARINE = 0xFE9A1E44,
	PICKUP_VEHICLE_ARMOUR_STANDARD = 0x13521DD4,
	PICKUP_VEHICLE_CUSTOM_SCRIPT = 0x661AF371,
	PICKUP_VEHICLE_CUSTOM_SCRIPT_LOW_GLOW = 0x3195FA6B,
	PICKUP_VEHICLE_CUSTOM_SCRIPT_NO_ROTATE = 0x37845ABA,
	PICKUP_VEHICLE_HEALTH_STANDARD = 0x6419E411,
	PICKUP_VEHICLE_HEALTH_STANDARD_LOW_GLOW = 0xB4CBFC00,
	PICKUP_VEHICLE_MONEY_VARIABLE = 0x2267DBF4,
	PICKUP_VEHICLE_WEAPON_APPISTOL = 0x256EE491,
	PICKUP_VEHICLE_WEAPON_ASSAULTSMG = 0x4FC5256C,
	PICKUP_VEHICLE_WEAPON_COMBATPISTOL = 0x8C0F737B,
	PICKUP_VEHICLE_WEAPON_GRENADE = 0x5985D162,
	PICKUP_VEHICLE_WEAPON_MICROSMG = 0x6DFF6B70,
	PICKUP_VEHICLE_WEAPON_MOLOTOV = 0x9B61A83E,
	PICKUP_VEHICLE_WEAPON_PISTOL = 0xD93F3079,
	PICKUP_VEHICLE_WEAPON_PISTOL50 = 0x31FB95FE,
	PICKUP_VEHICLE_WEAPON_SAWNOFF = 0x46CB54AA,
	PICKUP_VEHICLE_WEAPON_SMG = 0x0BD7C070,
	PICKUP_VEHICLE_WEAPON_SMOKEGRENADE = 0xE1AAF374,
	PICKUP_VEHICLE_WEAPON_STICKYBOMB = 0xEFD90F7B,
	PICKUP_WEAPON_ADVANCEDRIFLE = 0x5F532944,
	PICKUP_WEAPON_APPISTOL = 0xD975F9BA,
	PICKUP_WEAPON_ASSAULTRIFLE = 0xD919B569,
	PICKUP_WEAPON_ASSAULTRIFLE_MK2 = 0xFFE393D4,
	PICKUP_WEAPON_ASSAULTSHOTGUN = 0x098074E9,
	PICKUP_WEAPON_ASSAULTSMG = 0xB8F73C4B,
	PICKUP_WEAPON_AUTOSHOTGUN = 0xA290333F,
	PICKUP_WEAPON_BAT = 0x631B3559,
	PICKUP_WEAPON_BATTLEAXE = 0x14480AE9,
	PICKUP_WEAPON_BOTTLE = 0x094D9FE4,
	PICKUP_WEAPON_BULLPUPRIFLE = 0x7249242A,
	PICKUP_WEAPON_BULLPUPRIFLE_MK2 = 0xA8825252,
	PICKUP_WEAPON_BULLPUPSHOTGUN = 0x3E98BF1D,
	PICKUP_WEAPON_CARBINERIFLE = 0xC2AF8B50,
	PICKUP_WEAPON_CARBINERIFLE_MK2 = 0x3C8D8807,
	PICKUP_WEAPON_COMBATMG = 0xD8293551,
	PICKUP_WEAPON_COMBATMG_MK2 = 0x0324F48F,
	PICKUP_WEAPON_COMBATPDW = 0x2F7F85C0,
	PICKUP_WEAPON_COMBATPISTOL = 0x81D8E9A7,
	PICKUP_WEAPON_COMPACTLAUNCHER = 0x6895CA8D,
	PICKUP_WEAPON_COMPACTRIFLE = 0x43F0300C,
	PICKUP_WEAPON_CROWBAR = 0x32A3E225,
	PICKUP_WEAPON_DAGGER = 0x8ED05D6D,
	PICKUP_WEAPON_DBSHOTGUN = 0x157FCCBB,
	PICKUP_WEAPON_DOUBLEACTION = 0x946933E0,
	PICKUP_WEAPON_FIREWORK = 0x2E6341DB,
	PICKUP_WEAPON_FLAREGUN = 0x591BCAB0,
	PICKUP_WEAPON_FLASHLIGHT = 0x15140D79,
	PICKUP_WEAPON_GRENADE = 0xE9CE437B,
	PICKUP_WEAPON_GRENADELAUNCHER = 0xF73FBD4F,
	PICKUP_WEAPON_GUSENBERG = 0x365EF2B7,
	PICKUP_WEAPON_GolfClub = 0x69C100F4,
	PICKUP_WEAPON_HAMMER = 0x4AC18024,
	PICKUP_WEAPON_HATCHET = 0xB7E0DA7D,
	PICKUP_WEAPON_HEAVYPISTOL = 0x78EDD78B,
	PICKUP_WEAPON_HEAVYSHOTGUN = 0xB85AF9B9,
	PICKUP_WEAPON_HEAVYSNIPER = 0x1A88742D,
	PICKUP_WEAPON_HEAVYSNIPER_MK2 = 0x0A8163F8,
	PICKUP_WEAPON_HOMINGLAUNCHER = 0x6AE2597E,
	PICKUP_WEAPON_KNIFE = 0x08B8D9EA,
	PICKUP_WEAPON_KNUCKLE = 0x98318E6B,
	PICKUP_WEAPON_MACHETE = 0xBD0C4ED1,
	PICKUP_WEAPON_MACHINEPISTOL = 0x6B577ED1,
	PICKUP_WEAPON_MARKSMANPISTOL = 0x1EFD0989,
	PICKUP_WEAPON_MARKSMANRIFLE = 0xE2554CCF,
	PICKUP_WEAPON_MARKSMANRIFLE_MK2 = 0x45C51D23,
	PICKUP_WEAPON_MG = 0x11D26DCF,
	PICKUP_WEAPON_MICROSMG = 0xC9934322,
	PICKUP_WEAPON_MINIGUN = 0xB54E6766,
	PICKUP_WEAPON_MINISMG = 0xEEA44CAE,
	PICKUP_WEAPON_MOLOTOV = 0xC3FC570A,
	PICKUP_WEAPON_MUSKET = 0x3B382008,
	PICKUP_WEAPON_NIGHTSTICK = 0xDF9ABCFC,
	PICKUP_WEAPON_PETROLCAN = 0xA43F42B6,
	PICKUP_WEAPON_PIPEBOMB = 0xE2E0F1D1,
	PICKUP_WEAPON_PISTOL = 0xEA91B807,
	PICKUP_WEAPON_PISTOL50 = 0xF956A0A1,
	PICKUP_WEAPON_PISTOL_MK2 = 0x3ABD57C5,
	PICKUP_WEAPON_POOLCUE = 0x6C36760C,
	PICKUP_WEAPON_PROXMINE = 0xBA4B7099,
	PICKUP_WEAPON_PUMPSHOTGUN = 0x86500326,
	PICKUP_WEAPON_PUMPSHOTGUN_MK2 = 0x137671BF,
	PICKUP_WEAPON_RAILGUN = 0xC921115B,
	PICKUP_WEAPON_RAYCARBINE = 0x1CFFE9DB,
	PICKUP_WEAPON_RAYMINIGUN = 0xC9B2A820,
	PICKUP_WEAPON_RAYPISTOL = 0xC9D0FB96,
	PICKUP_WEAPON_REVOLVER = 0x4CA70DEB,
	PICKUP_WEAPON_REVOLVER_MK2 = 0x52F2CC36,
	PICKUP_WEAPON_RPG = 0xAC581E2E,
	PICKUP_WEAPON_SAWNOFFSHOTGUN = 0xB137BE25,
	PICKUP_WEAPON_SMG = 0x5D384A21,
	PICKUP_WEAPON_SMG_MK2 = 0x1DB367D4,
	PICKUP_WEAPON_SMOKEGRENADE = 0x5A04C3CE,
	PICKUP_WEAPON_SNIPERRIFLE = 0x98C74B66,
	PICKUP_WEAPON_SNSPISTOL = 0xB0769393,
	PICKUP_WEAPON_SNSPISTOL_MK2 = 0x66B72568,
	PICKUP_WEAPON_SPECIALCARBINE = 0x3172B8CE,
	PICKUP_WEAPON_SPECIALCARBINE_MK2 = 0x8F75AB24,
	PICKUP_WEAPON_STICKYBOMB = 0xEFBC7005,
	PICKUP_WEAPON_STONE_HATCHET = 0x892ABD83,
	PICKUP_WEAPON_STUNGUN = 0x9598EDD4,
	PICKUP_WEAPON_SWITCHBLADE = 0xADDECFC5,
	PICKUP_WEAPON_VINTAGEPISTOL = 0x2BB8CC22,
	PICKUP_WEAPON_WRENCH = 0x9EEDF9D0
};