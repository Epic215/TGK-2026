# Hexfire

Dokumentacja projektu gry — wersja prototypowa.  
Poniższy opis jest elastyczny: niektóre elementy są już zaimplementowane, inne zaplanowane na kolejne iteracje.

---

## 1. Krótki opis gry

**Tytuł roboczy:** Hexfire  

**Koncepcja:** Prototyp **3D bullet hell / twin-stick shootera** z perspektywy **top-down**, w którym gracz wciela się w **maga**. Gra rozgrywa się na w zamku z wrogami i bossem; celem jest przetrwanie i pokonanie przeciwników przy użyciu broni magicznych i uników. Koniec jest równoznaczny z pokonaniem wielkiego złotego mimick'a

**Inspiracje:**
- **Enter the Gungeon** — pętla walki w pokojach/arenie, zbieranie broni, dash z invincibility frames, czytelny HUD z slotami ekwipunku.
- Ogólnie gatunek **roguelite / arena shooter** (szybka akcja, uniki, różne typy broni).
- Estetyka magii i efektów VFX z pakietów Unity Asset Store (ogień, tarcze, pociski).

**Co jest charakterystyczne tej grze:**
- Mag z animacjami (chód, atak, obrona) zamiast „sztywnej” postaci.
- **System broni oparty na typach** (kula, kostury, miecz) z osobnymi umiejętnościami LPM/PPM — np. spread, chaos, serpentine, leczenie, odnowa many, magiczna tarcza, nova.
- Nowy kod gry w folderze `Assets/Hexfire/` (gracz, broń, UI), obok starszego prototypu w `Assets/Scripts/`.

**Stan projektu:** To **prototyp demonstracyjny**, nie pełna gra. Działa walka na jednej scenie (`SampleScene`). Do oddania zostają m.in. mapa, build `.exe`, screenshoty i linki do assetów (patrz **§9 TODO**).

---

## 2. Użyte narzędzia

| Element | Wybór |
|--------|--------|
| Silnik | **Unity 6** (6000.3.11f1) |
| Pipeline renderowania | **URP** (Universal Render Pipeline) |
| Język skryptowy | **C#** |
| System wejścia | **Unity Input System** (`InputSystem`) |
| Edytor UI | Unity UI (uGUI) + **TextMeshPro** |
| Platforma docelowa | **PC (Windows)** — projekt uruchamiany w edytorze Unity lub jako build `.exe` |

---

## 3. Opis mechaniki gry

### Świat i kamera
- **2.5D / 3D top-down** — postacie i otoczenie w 3D, rozgrywka jak w strzelance z góry (brak swobodnej kamery FPS).
- **Jedna arena / mapa ręczna** na scenie (ściany, korytarze, przestrzenie walki). Proceduralna mapa — **planowana**, niezaimplementowana.
- Kamera podąża za graczem (skrypt `CameraFollow`).

### Postać gracza (mag)
- **Ruch:** WASD, `CharacterController`, grawitacja i przyciąganie do podłoża.
- **Celowanie:** obrót w stronę kursora myszy.
- **Dash (Spacja):** szybki przeskok z cooldownem; podczas dasha **i-frames** (krótka niewrażliwość na obrażenia).
- **Atrybuty:** HP (domyślnie 100), mana (domyślnie 100, regeneracja w czasie).
- **Ekwipunek:** 3 sloty broni; podnoszenie z mapy klawiszem **E**, przełączanie **1 / 2 / 3**.

### System walki i bronie
Broń zdefiniowana jako **ScriptableObject** (`WeaponData` i klasy pochodne). Przykładowe typy w prototypie:

| Broń | LPM | PPM |
|------|-----|-----|
| Zielona kula ognia | Pojedynczy pocisk (0 MP) | Leczenie (+HP, koszt many) |
| Kostur 1 (rubinowy) | Spread — 3 pociski naraz (mana za cały strzał) | Odnowa many + efekt VFX |
| Kostur 2 (szafirowy) | Chaos — seria pocisków pod losowym kątem | „Shotgun” — wiele pocisków (wysoki koszt MP) |
| Miecz lodowy | Atak melee w zasięgu | Magiczna tarcza (i-frames + VFX) |
| Kostur 3 (szmaragdowy) | Serpentine — dwa pociski wijące się naprzemiennie (10 MP) | Verdant Nova — pierścień 18 pocisków wokół gracza (30 MP, cd ~4,5 s) |

Wzorce strzału (single / spread / chaos / serpentine) nawiązują do wcześniejszego prototypu `PlayerInventory`.

Pociski: prefaby z kolizją, obrażenia wrogów przez tag `Enemy` / komponenty zdrowia.

### Przeciwnicy i boss

W projekcie współistnieją **dwa systemy AI** — starszy (na większości wrogów na scenie) i nowy Hexfire (gotowy kod + prefab, do rozszerzenia na mapę).

#### Starszy system (`Assets/Scripts/`)

Używany przez **większość wrogów** na `SampleScene`.

| Skrypt | Rola |
|--------|------|
| `EnemyController` | Ruch + strzelanie |
| `EnemyHealth` | HP, obrażenia, śmierć |
| `BossController` | Boss — fazy, skoki, teleport, mandala, spirale |

**Ruch (`EnemyController`):** `Chase` (gonienie z zigzagiem), `Stationary` (stoi), `Strafe` (ruch na boki wokół gracza).

**Atak:** `Single` (pojedyncze pociski), `Burst` (seria pod kątem), `Spiral` (wir pocisków przez kilka sekund).

Wrog szuka gracza po tagu `Player`, obraca się w jego stronę, strzela z `firePoint` prefabem `EnemyBullet`.

#### Nowy system Hexfire (`Assets/Hexfire/Enemies/`)

Modułowy kontroler do konfiguracji **per prefab** w Inspectorze — bez zmiany kodu.

| Skrypt | Rola |
|--------|------|
| `HexfireEnemyController` | Ruch + wzorce ataku |
| `HexfireEnemyBulletSpawner` | Spawn pocisków wroga |
| `HexfireEnemyAnimator` | Animacje Haon (Ghost / Mimic / Chest Mimic) |
| `HexfireBossController` | Rozbudowany boss (fazy, mandala, skoki) — **kod gotowy**, scena na razie używa starego `BossController` |

**Tryby ruchu (`HexfireEnemyMoveMode`):**

| Tryb | Zachowanie |
|------|------------|
| `Chase` | Gonienie gracza z lekkim zigzagiem bocznym |
| `Stationary` | Bez ruchu (np. wieżyczka) |
| `Strafe` | Ruch na boki + korekta dystansu |
| `Retreat` | Cofa się, gdy gracz za blisko; podchodzi, gdy za daleko |
| `Orbit` | Okrąża gracza |
| `Kite` | Utrzymuje preferowany dystans (strzelanie z bezpiecznej odległości) |
| `Charge` | Okresowy szarż na gracza |

**Wzorce ataku (`HexfireEnemyAttackPattern`):** `Single`, `Burst`, `Spiral`, `Ring`, `Fan`, `Scatter`, `Cross`, `PulseRing`, `Alternating`, `Shotgun`, `Wave` — każdy z własnymi parametrami (cooldown, liczba pocisków, kąty itd.).

**Dodatkowo:** przewidywanie ruchu gracza (`aimLeadStrength`), opcja zatrzymania ruchu podczas ataku (`freezeMovementWhileAttacking`), pasek HP nad głową (`EnemyCanvas`).

**Prefab referencyjny:** `Assets/Hexfire/Enemies/Prefabs/EnemyNewFinal.prefab` (mimic-skrzynia + nowe skrypty). Na scenie jest **jedna** taka instancja; pozostałe wrogowie nadal na starym `EnemyController`.

#### Boss i warunek wygranej

- **Boss na scenie:** `BossController` (stary prototyp) — fazy zależne od HP, różne wzorce strzału.
- **Wygrana:** komponent `WinOnDestroy` na bossie — po zniszczeniu pokazuje panel wygranej (`GameOverMenu` w trybie win).

#### Co dalej z AI (opcjonalnie)

- Podmiana pozostałych wrogów na prefaby z `HexfireEnemyController` (różne `moveMode` + `attackPattern` per typ).
- Migracja bossa na `HexfireBossController`.
- Unikanie pocisków gracza — **niezaimplementowane**.

### Interfejs (HUD)
- Paski **HP** i **many** + tekst wartości.
- **Ring cooldownu** dasha.
- **Pasek 3 slotów** broni z ikonami; przycisk **„i”** z opisem LPM/PPM aktywnej broni.
- Skrypty w `Assets/Hexfire/UI/` (`PlayerHudWire`, `EquipmentBarHud`, `WeaponInfoPanel`).

### Menu i pętla gry
- **Menu główne** (`Menu` scena): start gry, wyjście (`MenuManager`).
- **Pauza (ESC):** panel wstrzymania, wznowienie, powrót do menu (`PauseMenu`) — działa na scenie; **do dopracowania wizualnie** (spójny styl z menu głównym).
- **Game Over:** panel po śmierci gracza (`GameOverMenu`) — przyciemnione tło, **Retry** (przeładowanie sceny) i **Exit** (menu główne).
- **Wygrana:** po zniszczeniu bossa (`WinOnDestroy`) — ten sam panel z komunikatem o wygranej.

### Sterowanie (skrót)

| Akcja | Klawisz |
|--------|---------|
| Ruch | W A S D |
| Celowanie | Mysz |
| Atak / umiejętność LPM | Lewy przycisk myszy |
| Umiejętność PPM | Prawy przycisk myszy |
| Dash | Spacja |
| Slot broni | 1 / 2 / 3 |
| Podnieś broń | E |
| Pauza | Escape |

---

## 4. Użyte assety

Większość grafiki, modeli, animacji, efektów i UI pochodzi z **Unity Asset Store** (oraz domyślnych zasobów Unity / TextMeshPro).

**Źródła (szczegółowe linki):**

| Kategoria | Pakiet / folder w projekcie | Uwagi |
|-----------|----------------------------|--------|
| Postać maga | `WizardPBR` | Model, animacje, kostury |
| Efekty magiczne | `Hovl Studio` (Magic effects pack, Procedural fire, itd.) | VFX, aury, tarcza |
| Pociski / VFX | `Unique_Projectiles_Volume_2` | Cząsteczki pocisków (wymaga URP) |
| Otoczenie / level kit | prefaby w scenie (`Wall`, `Corridor`, `EMPTY_SPACE`, itd.) | Zestawy z Asset Store / wcześniejszy import |
| UI | własne prefaby w `Assets/Hexfire/UI/` + elementy z pakietów UI | Część layoutu budowana skryptami edytora |

Linki:

- [WizardPBR](https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/battle-wizard-pbr-127652)
- [Haon](https://assetstore.unity.com/packages/3d/characters/creatures/haon-sd-creature-pack-311173)
- [Ikony](https://assetstore.unity.com/packages/2d/gui/icons/pixel-art-icon-pack-rpg-158343)
- [UI](https://assetstore.unity.com/packages/2d/gui/bloodlines-dark-ui-328721)
- [Rózne biblioteki i assety](https://github.com/VuxDzung/SEP490_SU26_Unity/tree/main/Assets/3rdParty/VFX/Hovl%20Studio/Procedural%20fire)

---

## 5. Wykorzystanie AI

- **Grafika menu** — część elementów interfejsu / koncepcji menu wspierana generatywnie (AI) przy projektowaniu wyglądu.
- **Logika gry** — **częściowy** udział AI przy pomocy w pisaniu i refaktoryzacji fragmentów kodu C# (system broni, HUD, setup gracza); ostateczna integracja, testy i decyzje projektowe — autor.

---

## 6. Uruchomienie gry

Należy pobrać z release paczke gry Hellfire.zip. Zawartość należy wypakować oraz uruchomić Hellfire.exe. Po wystartowaniu należy wcisnąć start.

---

## 7. Zrzuty ekranu

*Do uzupełnienia — jeden lub więcej reprezentatywnych screenshotów (walka, HUD, menu, boss).*


---

## Struktura kodu (skrót)

```
Assets/Hexfire/
├── Core/          — wspólne interfejsy (np. IDamageable)
├── Player/        — ruch, dash, HP, mana, ekwipunek, prefab Player_Mage
├── Weapons/       — broń, pociski, pickupy, dane ScriptableObject
├── Enemies/       — HexfireEnemyController, prefaby wrogów, animator
├── UI/            — HUD, pasek broni, panel informacji, game over / win
└── Editor/        — menu pomocnicze Hexfire (setup maga, animacje, materiały URP)

Assets/Scripts/    — starszy prototyp (wrogowie, boss, menu, pauza, kamera)
```
