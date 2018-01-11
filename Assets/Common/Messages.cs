/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/


public static class Msg
{
    public const string LabChanged = "LabChanged";
    public const string PlayerStatusChanged = "PlayerStatusChanged";
    public const string UsePotion = "UsePotion";
    public const string StockChanged = "StockChanged";
    public const string BuffUpdated = "BuffUpdated";
    public const string GainBuff = "GainBuff";
    public const string LoseBuff = "LoseBuff";

    public static class Player
    {
        public const string Spawn = "Spawn";
        public const string Ready = "Ready";
        public const string Damage = "Damage";
        public const string Heal = "Heal";
        public const string Dead = "Dead";
        public const string EnterArea = "InArea";
        public const string LeaveArea = "LeaveArea";

        public const string Request = "Player.Request";
        public const string Order = "Player.Order";
    }

    public static class GPS
    {
        public const string UpdatePOIMarks = "UpdatePOIMarks";
    }
}