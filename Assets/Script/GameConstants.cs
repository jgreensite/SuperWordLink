namespace AssemblyCSharp
{
    public class CS
    {
        //Names of Objects
        public const string OBJ_NAME_ROOT_CARD = "Card Root";

        //Layers
        public const string OBJ_LOCATION_LAYER_GAMEBOARD = "Board";
        public const string OBJ_LOCATION_LAYER_PLAYERHAND = "Hand";

        //Tag make the same as layers, note that these must be set in the editor
        public const string OBJ_LOCATION_TAG_GAMEBOARD = OBJ_LOCATION_LAYER_GAMEBOARD;
        public const string OBJ_LOCATION_TAG_PLAYERHAND = OBJ_LOCATION_LAYER_PLAYERHAND;

        //Card Location
        public const string CAR_LOCATION_GAMEBOARD = "card is on the Gameboard";
        public const string CAR_LOCATION_RED_DECK = "card is in the Red Deck";
        public const string CAR_LOCATION_BLUE_DECK = "card is in the Blue Deck";
        public const string CAR_LOCATION_RED_HAND = "card is in the Red Hand";
        public const string CAR_LOCATION_BLUE_HAND = "card is in the Blue Hand";
        public const string CAR_LOCATION_DISCARD_DECK = "card is in the Discard Deck";

        //Card Status
        public const string CAR_REVEAL_HIDDEN = "card face is hidden";
        public const string CAR_REVEAL_SHOWN = "card face is shown";

        //Card when playable
        public const string CWP_PLAY_PLAYER_TURN = "on the players turn";
        public const string CWP_PLAY_ANY_TURN = "on any players turn";

        //Card effect playable
        //Effects
        public const string CEP_EFFECT_REVEAL_CARD = "reveals a specific card on the board";
        public const string CEP_EFFECT_CHANGE_CARD = "randomly changes a card on the board";
        public const string CEP_EFFECT_REMOVE_CARD = "randomly removes a card from the board";

        //Affects
        public const string CEP_AFFECT_GAMEBOARD = "affects the game board";
        public const string CEP_AFFECT_OWN_DECK = "affects your deck";
        public const string CEP_AFFECT_OPPONENT_DECK = "affects your opponent's deck";
        public const string CEP_AFFECT_OWN_HAND = "affects your hand";
        public const string CEP_AFFECT_OPPONENT_HAND = "affects opponent's hand";

        //Number of Cards in player's hands
        public const int CSCARDHANDDIM = 3;

        //Gameboard dimensions
        public const int CSGRIDXDIM = 5;
        public const int CSGRIDZDIM = 5;

        //Game Winstates
        public const string NONEWIN = "nonewin";
        public const string BLUEWIN = "bluewin";
        public const string REDWIN = "redwin";

        //Card Class and Team constants
        public const string BLUE_TEAM = "blue";
        public const string RED_TEAM = "red";
        public const string DEATH_TEAM = "death";
        public const string CIVIL_TEAM = "civil";
        public const int IDCARDFRONTMATERIAL = 1;
        public const int IDCARDBACKMATERIAL = 0;
        public const int NUMCARDMATERIALS = 4;

        //Flow Control constants
        public const string GOOD = "good";
        public const string BAD = "bad";
        public const string EMPTY = "empty";
        public const string ERROR = "error";

        //UI Label constants
        public const string UILABELPANZOOM = "Pan/Zoom";
        public const string UILABELSELECT = "Select";
        public const string UILABELBACK = "<-Back";
        public const string UILABELCANCEL = "Cancel";
        public const string UILABELSTART = "Start";
        public const string UILABELHOSTLOCAL = "Host Local";
        public const string UILABELHOSTREMOTE = "Host Remote";
        public const string UILABELCONNECT = "Connect";


        //Networking constants
        public const string GAMESERVERLOCALADDRESS = "127.0.0.1";

        //TODO - Make Server and port selectable
        public const string GAMESERVERREMOTEADDRESS = "35.177.228.70";
        public const int GAMESERVERPORT = 6321;

        //Build script constants
        public const string SERVERSCENECOLLECTION = "server";
        public const string CLIENTSCENECOLLECTION = "client";
        public const string OSXBUILDPLATFORM = "OSX";
        public const string UNXBUILDPLATFORM = "UNX";
        public const string WINBUILDPLATFORM = "WIN";
        public const string ANDBUILDPLATFORM = "AND";

        public const string IOSBUILDPLATFORM = "IOS";

        //Put all the Effects in here to make it is easy to ramdonly pick one
        public static string[] CEP_EFFECTS = {CEP_EFFECT_REVEAL_CARD, CEP_EFFECT_CHANGE_CARD, CEP_EFFECT_REMOVE_CARD};
    }
}