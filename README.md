# zdroj_GUI

Súbory Zdroj.cs a ZdrojSkript.cs s triedami pre riadenie zdroja sú v hlavnom priečinku koncolovej aplikácie zdroj_console,
kde boli vyvíjané a testované. Implementované je riadenie zdroja cez vlastnosti triedy Zdroj a zároveň riadenie
zdroja zo "skriptovacieho" súboru .csv. Kompletná komunikácia so zdrojom cez sériový port je ukladaná do logovacieho
súboru. Viac informácií je v komentároch daných súborov. Obidve triedy vyvolajú výnimku, ak nastane akákoľvek chyba,
preto by mali byť tieto triedy v hlavnom programe volané pomocou try{} a catch{}. Výnimka už má naformátovaný text,
takže bude stačiť zobraziť tento text vo vyskakovacom okne (MessageBox).

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

Uloženie nastavenia zdroja do súboru (nastavenie ochrán, napätie, prúd, strmosť nábehu).

Načítanie nastavenia zdroja zo súboru (nastavenie ochrán, napätie, prúd, strmosť nábehu).

HOTOVO - Nastavovanie napätia a prúdu podľa "programu" v súbore. Súbor bude obsahovať čas od spustenia a hodnutu napätia a prúdu,
         ktorá má byť vtedy nastavená. Formát súboru nie je kritický, mne sa napr. páči csv.

Cez hornú lištu menu, možnosť otvoriť nové okno s kalibráciou zdroja. Okno bude obsahovať dva samostatné typy kalibrácie.
Prvý typ je kalibrácia napätia, druhý kalibrácia prúdu. Užívateľ zadá začiatočnú a konečnú hodnotu napätia resp. prúdu,
medzi ktorými je vykonaná kalibrácia.

Pri odpojení zdoja, prepnutí do analógového režimu, alebo výpadku komunikácie nastane upozornenie o danej chybe.

Program by mal obsahovať nejaké tlačidlo na pripojenie a odpojenie sa od zdroja. (Pri spustení programu sa automaticky pokúsi
pripojiť ku zdroju. Ak sa to nepodarí, tak vypíše chybu a všetky ovládacie prvky zostanú neaktívne. Potom užívateľ môže
pripojiť zdroj a stlačiť tlačidlo, aby sa program znovu pokúsil pripojiť ku zdroju. Ak sa to podarí, ovládacie prvky
sa aktivujú a program nabehne do normálneho režimu, v ktorom sa používa. Na konci práce užívateľ stlačí tlačidlo,
odpojí sa od zdroja a znovu sa všetky prvky deaktivujú. Odpojenie od zdroja musí byť aj automatické pri vypnutí
ovládacieho programu.

Ak nie je zdroj pripojený, mohli by byť ovládacie prvky neaktívne (šedé), aby bolo jasné, že sa s nimi nedá pracovať.

Záložka "info" v hornej lište menu s informáciami o verzii programu a krátkym textom. (Aby mala lišta viac záložiek)

Ak by toho bolo málo, môže sa pridať prepínanie jazyka programu (EN, CZ, SK).