public enum GHM_TransmitFunktion : short
{
	AnzeigewertLesen		= 0,
	AnzeigewertSetzen,
	//2
	SystemstatusLesen		= 3,
	ACK,
	//5
	MinwertspeicherLesen	= 6,
	MaxwertspeicherLesen	= 7,
	//8
	//9
	//10
	//11
	IDNummerLesen			= 12,
	AdresseZuweisen,
	AdresseLesen,
	//15
	//16
	//17
	//18
	//19
	//20
	//21
	MinAlarmgrenzeLesen		= 22,
	MaxAlarmgrenzeLesen,
	AlarmverzögerungLesen,
	AlarmfunktionLesen,
	//26
	//27
	//28
	//29
	//30
	//31
	KonfigurationsflagLesen	= 32,
	//33
	//34
	//35
	//36
	//37
	//38
	//39
	//40
	//41
	SchaltausgangEinschaltpunktLesen = 42,
	SchaltausgangEinschaltpunktSetzen,
	SchaltausgangAusschaltpunktLesen,
	SchaltausgangAusschaltpunktSetzen,
	//46
	//47
	//48
	//49
	SchaltausgangVerzögerungLesen = 50,
	SchaltausgangVerzögerungSetzen,
	SchaltausgangFunktionLesen,
	SchaltausgangFuntkonSetzen,
	//54
	//...
	//99
	StatusLöschenOderSetzen = 100,
	//101
	MinAlarmgrenzeSetzen = 103,
	MaxAlarmgrenzeSetzen,
	AlarmverzögerungSetzen,
	AlarmfunktionSetzen,
	//106
	//...
	//159
	KonfigurationsflagSetzen = 160,
	//161
	//...
	//173
	MinwertspeicherSetzen = 174,
	MaxwertspeicherSetzen,
	MinMessbereichLesen,
	MaxMessbereichLesen,
	MessbereichEinheitLesen,
	MessbereichDezimalpunktLesen,
	MessbereichMessartLesen,
	//181
	//...
	//190
	AnzeigeMessartSetzen = 191,
	MinAnzeigebereichSetzen,
	MasAnzeigebereichSetzen,
	AnzeigeEinheitSetzen,
	AnzeigeDezimalpunktSetzen,
	//196
	//197
	//198
	AnzeigeMessartLesen = 199,
	MinAnzeigebereichLesen,
	MaxAnzeigebereichLesen,
	AnzeigeeinheitLesen,
	BatteriezustandLesen,
	AnzeigeDezimalpunktLesen,
	AnzeigeMessartErweiterungLesen,
	//206
	//207
	KanalzahlLesen = 208,
	MasterFunktion209,
	ElektrodenzustandLesen,
	Masterfunktion211,
	Masterfunktion212,
	Masterfunktion213,
	SteigungsKorrekturLesen = 214,
	SteigungsKorrekturSetzen,
	OffsetkorrekturLesen,
	OffsetkorrekturSetzen,
	KorrekturfaktorFürOberflächenMessungLesen,
	KorrekturfaktorFürOberflächenMessungSetzen,
	AltitudeLesen,
	AltitudeSetzen,
	AbschaltverzögerungLesen,
	AbschaltverzögerungSetzen,
	LoggerdatenLesen = 224,
	LoggerzyklusLesen,
	LoggerzyklusSetzen,
	LoggeraufzeichnnungStarten,
	AnzahlDerLoggerDatenLesen,
	LoggerzustandLesen,
	LoggerLesezeigerSetzen,
	LoggerStartStoppzeitLesen,
	LoggerStartStoppzeitSetzen,
	EchtzeituhrLesen = 233,
	EchtzeituhrSetzen,
	ZeitpunktDerLetztenZeitkorrekturLesen,
	LoggerspeichergrösseLesen,
	LoggerFilezahlLesen,
	LoggerFilezeigerSetzen,
	LoggerFilelesenAufzeichnungInfo,
	SonsormodulRücksetzen,
	SensormodulNeustart,
	//242
	//...
	//247
	GerätespezifischeFunktion = 248,
	//249
	//...
	//253
	ProgrammkennungLesen = 254,
	//255
	//...
	//259
	LoggerdatenLesenManuellerLogger = 260,
	GrösseFreierLoggerspeicherLesen,
	AnzahlFreieLoggerfilesLesen,
	LoggerGilelesenKanalinfo
}

public enum Sprache : short
{
	Deutsch = 0x0000,
	Englisch = 0x1000,
	Tschechisch = 0x2000,
	Spanisch = 0x3000,
}

public enum Fehlermeldungen : short
{
	NegativeQuittung = -38,
	KonnteNachrichtNichtSendenCTSTimeout,
	RückgabewetIstFehlercode,
	EchodatenUnvollständig,
	KeineEchodatenEmpfangen,
	WertAusserhalbDesGültigenBereichs,
	FFeldNichtKorrekt,
	DezimalPunktInformationenUngültig,
	
	ÜbergebenerCodeUngültig = -30,
	EchodatenNichtIdentisch,
	Testcode,
	KonntePortNichtSchliessen,
	RückgeleseneAdresseFalsch,
	CRCCodeFalsch,
	NachrichtenlängeFalsch,
	SensormodulAntwortetNicht,
	KonnteNachrichtNichtSenden,
	KonnteSchnittstelleNichtInitialisieren,
	KonnteDCBNichtBilden = -20,

	NichtUnterstützteBaudrate = -12,
	UngültigeBytegrösse = -11,
	HardwareNichtVorhanden = -10,

	PegelwandlerWirdNichtUnterstützt = -6,
	FehlerInStandardParametern,
	KannKeineWarteschlangeEinrichten,
	PortNichtBereit,
	PortBereitsGeöffnet,
	UngültigePortNummer,
	OK = 0,	
}