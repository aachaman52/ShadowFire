namespace ShadowFire.Core
{
    public enum GameState
    {
        MainMenu,
        InGame,
        WaveCountdown,
        UpgradePause,
        Paused,
        GameOver,
        LevelComplete
    }

    public enum AttributeType
    {
        Health,
        Armor,
        Stamina,
        Movement
    }

    public enum WeaponUpgradeType
    {
        Damage,
        FireRate,
        Magazine,
        Reload
    }

    public enum WeaponType
    {
        Rifle,
        SMG,
        Sniper,
        Shotgun,
        RocketLauncher
    }

    public enum FireMode
    {
        FullAuto,
        SemiAuto,
        Burst,
        BoltAction
    }

    public enum EnemyType
    {
        Zombie,
        Runner,
        Tank,
        Shooter,
        Boss
    }

    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    public enum UpgradeType
    {
        DamageBoost,       // +20% Damage
        FasterReload,      // -25% Reload Time
        BiggerMagazine,    // +30% Mag Capacity
        FasterSprint,      // +20% Sprint Speed
        MaxHealth,         // +25 Max HP + heal 25
        ArmorBoost,        // +15 Armor
        CriticalChance,    // +15% Crit Chance
        FireRateBoost,     // +20% Fire Rate
        ExplosiveAmmo,     // Bullets cause mini-explosions
        Lifesteal          // 10% damage converted to HP
    }

    public enum PickupType
    {
        HealthPack,
        AmmoBox,
        ArmorPlate,
        XpOrb
    }

    public enum HitType
    {
        Default,
        Critical,
        Explosive,
        Melee
    }
}
