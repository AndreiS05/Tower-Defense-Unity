# Tower Defense 2D (Unity / C#)

Joc de strategie de tip **Tower Defense** realizat în **Unity** (2D) cu logica scrisă
integral în **C#**. Proiectul implementează un sistem de gestionare a resurselor și o
logică de apărare automatizată: inamicii parcurg valuri (*Waves*) pe o traiectorie
predefinită (*Waypoints*), iar jucătorul plasează strategic unități de apărare (*Turrets*)
pentru a proteja un punct vital (baza).

## Tehnologii

- **Motor grafic:** Unity 2022.3 LTS (2D)
- **Limbaj:** C#
- **Mediu de dezvoltare:** Visual Studio / Visual Studio Code

## Cum se rulează

1. Deschide **Unity Hub** → **Add** → selectează folderul acestui proiect.
   - Recomandat: Unity **2022.3 LTS**. Dacă ai altă versiune 2021.3+ / 2023.x, Unity Hub
     va propune deschiderea cu versiunea instalată — acceptă (proiectul nu folosește API-uri
     dependente de versiune).
2. Deschide scena `Assets/Scenes/SampleScene.unity` (sau orice scenă goală).
3. Apasă **Play**.

> Întreaga scenă (cameră, traseu, grilă, valuri, ture, interfață) este construită
> automat din cod la pornire (`GameBootstrap`), deci jocul este complet jucabil
> imediat, fără configurări manuale în Editor și fără asset-uri grafice externe
> (sprite-urile sunt generate procedural).

## Cum se joacă

- **Bani (resurse):** începi cu 200. Câștigi bani distrugând inamici (recompensă mică pe
  inamic — economie echilibrată ca să nu devină prea ușor după multe ture). La fiecare
  nivel terminat primești un bonus.
- **Vieți:** baza are 20 de vieți. Fiecare inamic care ajunge la capătul traseului scade
  vieți (boss-ul scade mult mai multe).
- **Construire:** alege un tip de tură din magazinul de jos, apoi dă **click** pe o
  celulă verde liberă pentru a o plasa (dacă ai suficienți bani).
- **Obiectiv:** treci toate cele **3 niveluri**. Dacă viețile ajung la 0 → înfrângere.

## Niveluri și boss

Campania are **3 niveluri**, fiecare cu **8 valuri**:

| Nivel | Rute | Dificultate | Final |
|-------|------|-------------|-------|
| 1 | 1 rută | normală | Boss la valul 8 |
| 2 | 2 rute (inamicii aleg aleatoriu) | mai grea | Boss la valul 8 |
| 3 | 3 rute (inamicii aleg aleatoriu) | cea mai grea | Boss final la valul 8 |

- La nivelurile cu mai multe rute, fiecare inamic alege **aleatoriu** o rută spre bază.
- **Boss-ul** apare la ultimul val al fiecărui nivel: este mult mai mare, are foarte multă
  viață și e **evidențiat vizual** (corp închis la culoare, contur auriu strălucitor, miez
  pulsatil) ca să se distingă clar de inamicii de rând.
- Punctele de **apariție** (portal cyan rotativ) și **baza** (romb roșu cu inel auriu și
  miez pulsatil) au grafică dedicată.

### Tipuri de ture (unele se deblochează pe niveluri)

| Tură       | Cost | Deblocare | Rază   | Rol                                         |
|------------|------|-----------|--------|---------------------------------------------|
| Mitralieră | 50   | Nivel 1   | medie  | volum de foc pe ținte slabe                 |
| Tun        | 100  | Nivel 1   | medie  | daune mari pe lovitură                       |
| Lunetist   | 150  | Nivel 1   | mare   | daune f. mari la distanță                    |
| Mortieră   | 175  | **Nivel 2** | medie | **daune de zonă (AoE)** — lovește tot grupul de pe tile-urile din jur |
| Inferno    | 275  | **Nivel 3** | f. mare | **lovește 3 inamici simultan**; rază mai mare decât Mitralieră/Tun; cea mai scumpă |

> Armele blocate apar în magazin cu eticheta „Nivel X" și se activează automat când
> ajungi la nivelul respectiv (un banner anunță arma nouă).

## Structura proiectului

```
Assets/
  Scenes/
    SampleScene.unity        # scena de pornire
  Scripts/
    Core/
      GameManager.cs         # resurse, vieți, stare (singleton + evenimente)
      GameBootstrap.cs       # creează managerii + UI la pornire
      TextureFactory.cs      # generează sprite-uri (pătrat / cerc / inel) procedural
      Decor.cs               # componente Rotate + Pulse pentru portal/bază/boss
    Levels/
      LevelDefinition.cs     # definiția campaniei (rute, dificultate, culori)
      LevelBuilder.cs        # construiește grila, traseele, portalurile și baza
      LevelManager.cs        # progresia între niveluri + victoria finală
    Path/
      WaypointPath.cs        # traiectoria predefinită a inamicilor
    Enemies/
      Enemy.cs               # mișcare pe traseu, viață, recompensă, daune; boss
      WaveSpawner.cs         # 8 valuri, rute aleatorii, boss la valul 8
    Turrets/
      TurretBlueprint.cs     # definiția unui tip de tură (cost + statistici)
      Turret.cs              # țintire automată + tragere
      Bullet.cs              # proiectil teleghidat
    Build/
      Node.cs                # celulă pe care se construiește
      BuildManager.cs        # selecție tură + plasare la click + cheltuire resurse
    UI/
      GameUI.cs              # interfață (bani / vieți / val / magazin / ecran final)
ProjectSettings/             # configurări proiect Unity
Packages/                    # dependențe (manifest)
```

## Arhitectură (pe scurt)

- **GameManager** — singleton care ține resursele, viețile și starea jocului și emite
  evenimente (`OnMoneyChanged`, `OnLivesChanged`, `OnStateChanged`) pentru actualizarea UI-ului.
- **WaypointPath** — lista de puncte care formează traseul.
- **Enemy / WaveSpawner** — inamicii se înregistrează într-un registru static folosit de
  ture pentru țintire; spawner-ul generează valuri progresiv mai dificile.
- **Turret / Bullet** — turele caută cea mai apropiată țintă din rază și trag proiectile
  teleghidate.
- **Node / BuildManager** — gestionează plasarea turelor pe grilă și cheltuirea resurselor.
- **GameUI** — construiește interfața (uGUI) din cod și reacționează la evenimentele jocului.
