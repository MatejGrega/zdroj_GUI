# zdroj_GUI

Súbory Zdroj.cs a ZdrojSkript.cs s triedami pre riadenie zdroja sú v hlavnom priečinku koncolovej aplikácie zdroj_console,
kde boli vyvíjané a testované. Implementované je riadenie zdroja cez vlastnosti triedy Zdroj a zároveň riadenie
zdroja zo "skriptovacieho" súboru .csv. Kompletná komunikácia so zdrojom cez sériový port je ukladaná do logovacieho
súboru. Viac informácií je v komentároch daných súborov. Obidve triedy vyvolajú výnimku, ak nastane akákoľvek chyba,
preto by mali byť tieto triedy v hlavnom programe volané pomocou try{} a catch{}. Výnimka už má naformátovaný text,
takže bude stačiť zobraziť tento text vo vyskakovacom okne (MessageBox).

NuGet balíček System.IO.Ports;

Požiadavky na ovládací program pre laboratórny zdroj:

Grafické rozhranie, ktoré v hlavnom okne stále ukazuje aktuálne nastavené a merané hodnoty napätia a prúdu.

Nastavenie napätia v hlavnom okne.

Nastavenie prúdu v hlavnom okne.

V hlavnom okne veľký vypínač na zapnutie a vypnutie výstupu laboratórneho zdroja.

V hlavnom okne veľká indikácia zapnutého výstupu zdroja, ktorá je priebežne aktualizovaná.

V hlavnom okne nastavovanie ochrán: vypnuté, OCP, OVP pomocou tlačidiel typu radiobutton (vždy iba jedno aktívne).

V hlavnom okne nastavovanie strmosti nábehu.

Grafické zobrazenie priebehu napätia a prúdu v hlavnom okne. (Ako osciloskop v režime roll)

HOTOVO - Ukladanie komunikácie s laboratórnym zdrojom do logovacieho súboru, ktorý môže byť zobrazený v spodnej časti hlavného okna.

(Nerobiť) - Uloženie nastavenia zdroja do súboru (nastavenie ochrán, napätie, prúd, strmosť nábehu).

(Nerobiť) - Načítanie nastavenia zdroja zo súboru (nastavenie ochrán, napätie, prúd, strmosť nábehu).

HOTOVO - Nastavovanie napätia a prúdu podľa "programu" v súbore. Súbor bude obsahovať čas od spustenia a hodnutu napätia a prúdu,
         ktorá má byť vtedy nastavená. Formát súboru nie je kritický, mne sa napr. páči csv.

(Nepodstatné, môže sa vynechať) - Cez hornú lištu menu, možnosť otvoriť nové okno s kalibráciou zdroja. Okno bude obsahovať dva samostatné typy kalibrácie.
Prvý typ je kalibrácia napätia, druhý kalibrácia prúdu. Užívateľ zadá začiatočnú a konečnú hodnotu napätia resp. prúdu,
medzi ktorými je vykonaná kalibrácia.

Pri odpojení zdoja, prepnutí do analógového režimu, alebo výpadku komunikácie nastane upozornenie o danej chybe.

Program by mal obsahovať nejaké tlačidlo na pripojenie a odpojenie sa od zdroja. (Pri spustení programu sa automaticky pokúsi
pripojiť ku zdroju. Ak sa to nepodarí, tak vypíše chybu a všetky ovládacie prvky zostanú neaktívne.) Potom užívateľ môže
pripojiť zdroj a stlačiť tlačidlo, aby sa program znovu pokúsil pripojiť ku zdroju. Ak sa to podarí, ovládacie prvky
sa aktivujú a program nabehne do normálneho režimu, v ktorom sa používa. Na konci práce užívateľ stlačí tlačidlo,
odpojí sa od zdroja a znovu sa všetky prvky deaktivujú. Odpojenie od zdroja musí byť aj automatické pri vypnutí
ovládacieho programu.

Ak nie je zdroj pripojený, mohli by byť ovládacie prvky neaktívne (šedé), aby bolo jasné, že sa s nimi nedá pracovať.

(Nerobiť) - Záložka "info" v hornej lište menu s informáciami o verzii programu a krátkym textom. (Aby mala lišta viac záložiek)

(Nerobiť) - Ak by toho bolo málo, môže sa pridať prepínanie jazyka programu (EN, CZ, SK).








SCPI communication uses virtual serial port done by USB:
Definitions:
	Angle brackets < >		Items within angle brackets are parameter abbreviations. For example,
							<NR1> indicates a specific form of numerical data.
							
	Vertical bar |			Vertical bars separate one of two or more alternative parameters. For
							example, 0|OFF indicates that you may enter either "0" or "OFF" for the
							required parameter.
							
	Square brackets [ ]		Items within square brackets are optional. The representation
							[SOURce]:CURRent means that SOURce may be omitted.
							
	Braces { }				Group. One element is required
							
Types of parameters:
	boolean					0 or OFF; ON or nonzero value
	NR1 numeric				integers e.g. 0, 1, -7
	NR2 numeric				decimal numbers e.g. 5.48, 0.986438, -68.4507
	NR3 numeric				floating point numbers e.g. 4.389E-5, -1.687E2
	NRf numeric				flexible decimal number that can be type NR1, NR2, or NR3.

Each command is terminated by line feed '\n' (0x0A). Each command except *IDN? could beggin with colon ':'.
Available commands, responses and their meaning:							

*IDN?	returns: "MATEJ GREGA,Power supply 0-30V 1A,1,1.0"
*RST	performs CURR MIN; VOLT MIN; OUTP OFF; CURR:PROT:STAT OFF; VOLT:PROT:STAT OFF; VOLT:SR 3000

[SOURce:]CURRent[:LEVel][:IMMediate][:AMPLitude] {<NRf>|MIN|MAX}		set current value in ameres
[SOURce:]CURRent[:LEVel][:IMMediate][:AMPLitude]?	returns <NR2>		return set value of current in amperes
[SOURce:]CURRent:PROTection:STATe <boolean>								turn on/off OCP - enabling OCP disables OVP
[SOURce:]CURRent:PROTection:STATe?					returns<0|1>		return state of OCP (on/off)
[SOURce:]OUTPut[:STATe] <boolean>										enable or disable output of power supply
[SOURce:]OUTPut[:STATe]?							returns <0|1>
[SOURce:]VOLTage[:LEVel][:IMMediate][:AMPLitude] {<NRf>|MIN|MAX}		set voltge value in volts
[SOURce:]VOLTage[:LEVel][:IMMediate][:AMPLitude]?	returns <NR2>		return set value of voltage in volts
[SOURce:]VOLTage:PROTection:STATe <boolean>								turn on/off OVP - enabling OVP disables OCP
[SOURce:]VOLTage:PROTection:STATe?					returns <0|1>		return sate of OVP (on/off)
[SOURce:]VOLTage:SLEW {<NR1>|MIN|MAX}									set slew rate of voltage at the output
[SOURce:]VOLTage:SLEW?								returns <NR1>		return slew rate of voltage at the output

MEASure:CURRent[:DC]?				returns <NR2>		measure actual output current; returned value in amperes
MEASure:POWer[:DC]?					returns <NR2>		measure actual output power; returned value in watts
MEASure:VOLTage[:DC]?				returns <NR2>		measure actual output voltage; returned value in volts

SYSTem:CALibrate?					returns <0|1>		returns 1 if calibration is in progress
SYSTem:CALibrate:CURRent {<NRf>|MIN},{<NRf>|MAX}		initiate self calibration of current in specified range
SYSTem:CALibrate:VOLTage {<NRf>|MIN},{<NRf>|MAX}		initiate self calibration of voltage in specified range
SYSTem:MODE:DIGital?				returns <0|1>		return 1 if power supply is controlled digitally


There are two protections available in the power supply - OCP, OVP - over current and over voltage protection.
Output is disabled when OCP is enabled and current reaches its limit defined by SOUR:CURR:LEV:IMM:AMPL.
Output is disabled when OVP is enabled and voltage reaches its limit defined by SOUR:VOLT:LEV:IMM:AMPL.
Only one protection (OCP or OVP) can be active at the same time, because OCP and OVP trigger levels
are identical with levels of output current or voltage respectively.