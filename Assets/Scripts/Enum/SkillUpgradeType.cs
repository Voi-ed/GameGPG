public enum SkillUpgradeType
{
    None = 0,

    // ------ Dash Tree -------
    Dash = 1, // Dash to avoid damage
    Dash_CloneOnStart = 2, // Create a clone when dash starts
    Dash_CloneOnStartAndArrival = 3, // Create a clone when dash starts and ends
    Dash_ShardOnShart = 4, // Create a shard when dash starts
    Dash_ShardOnStartAndArrival = 5, // Create a shard when dash starts and ends

    // ------ Shard Tree -------
    Shard = 6, // The shard explodes when touched by an enemy or time goes up
    Shard_MoveToEnemy = 7, // Shard will move towards nearest enemy
    Shard_Multicast = 8, // Shard ability can have up to N charges. You can cast them all in a raw
    Shard_Teleport = 9, // You can swap places with the last shard you created
    Shard_TeleportHpRewind = 10, // When you swap places with shard, your HP % is same as it was when you created shard.

    // ------ Sword Throw Tree -------
    SwordThrow = 11, // You can throw sword to damage enemies from range
    SwordThrow_Spin = 12, // Your sword will spin at one point and damage enemies. Like a chainsaw
    SwordThrow_Pierce = 13, // Pierce sword will pierce N targets
    SwordThrow_Bounce = 14, // Bounce sword will bounce between enemies

    // ------ Time Echo Tree -------
    TimeEcho = 15,  // Create a clone of a player. It can take damage from enemies.
    TimeEcho_SingleAttack = 16, // Time Echo can perform a single attack
    TimeEcho_MultiAttack = 17, // Time Echo can perform N attacks
    TimeEcho_ChanceToDuplicate = 18, // Time Echo has a chance to create another time echo when attacks
    TimeEcho_HealWisp = 19, // When time echo dies it creates a wips that flies towards the player to heal it.
    TimeEcho_CleanseWisp = 20, // Wisp will now remove negative effects from player
    TimeEcho_CooldownWisp = 21, // Wisp will reduce cooldown of all skills by N second. 

    // ------ Domain Expansion Tree -------
    Domain_SlowingDown = 22, // Create an area in which you slow down enemies by 90-100% . You can freely move and fight.
    Domain_EchoSpam = 23, // You can no longer move, but you spam enemy with Time Echo ability
    Domain_ShardSpam = 24, // You can no longer move, but you spam enemy with Time Shard ability

    // ------ New Expansions -------
    Dash_FireTrail = 25, // Create a fire trail along dash path
    Dash_InfernalCrash = 26, // Fire explosion with knockback at dash end
    SwordThrow_Thunder = 27, // Lightning sword that strikes lightning and chains shock to nearby enemies
    SwordThrow_Frost = 28, // Ice sword that creates a blizzard vortex, pulls enemies, chills and freezes them
    Domain_ChronoStasis = 29, // Freeze time 100%, accumulate all incoming damage, and unleash a catastrophic burst upon collapse
    Domain_SwordBarrage = 30 // Wan Jian Gui Zong - Ten Thousand Swords: Levitate and summon an epic barrage of curved flying swords
}

