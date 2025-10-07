using Godot;
using System;

public partial class BookItem : ActiveItem
{
    [Export] public int DamageBonus = 1; // 子弹伤害加成
    [Export] public int SelfDamage = 2;   // 自身伤害

    public BookItem()
    {
        ItemName = "书";
        Description = "牺牲生命换取伤害";
        EnergyCost = 1;
        MaxEnergy = 4;
    }

    public override void UseItem(Player player)
    {
        base.UseItem(player);

        // 对玩家造成伤害
        player.TakeDamage(SelfDamage);

        // 增加子弹伤害
        player.ExtraDamage += 1;

        GD.Print($"使用书,受到 {SelfDamage} 伤害，子弹伤害 +{DamageBonus}");
    }
    
}