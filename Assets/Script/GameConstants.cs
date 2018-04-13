using System;

namespace AssemblyCSharp
{
	public class CS
	{
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
	}
}

