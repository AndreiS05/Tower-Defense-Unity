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

- **Bani (resurse):** începi cu 200. Câștigi bani distrugând inamici.
- **Vieți:** baza are 20 de vieți. Fiecare inamic care ajunge la capătul traseului scade o viață.
- **Construire:** alege un tip de tură din magazinul de jos, apoi dă **click** pe o
  celulă verde liberă pentru a o plasa (dacă ai suficienți bani).
- **Obiectiv:** rezistă tuturor celor 8 valuri. Dacă viețile ajung la 0 → înfrângere.

### Tipuri de ture

| Tură       | Cost | Rază  | Cadență | Daune | Rol                          |
|------------|------|-------|---------|-------|------------------------------|
| Mitralieră | 50   | medie | rapidă  | mici  | volum de foc pe ținte slabe  |
| Tun        | 100  | medie | lentă   | mari  | daune pe burst               |
| Lunetist   | 150  | mare  | lentă   | f. mari| acoperă distanțe lungi      |

## Structura proiectului

```
Assets/
  Scenes/
    SampleScene.unity        # scena de pornire
  Scripts/
    Core/
      GameManager.cs         # resurse, vieți, stare (singleton + evenimente)
      GameBootstrap.cs       # construiește nivelul din cod la pornire
      TextureFactory.cs      # generează sprite-uri (pătrat / cerc) procedural
    Path/
      WaypointPath.cs        # traiectoria predefinită a inamicilor
    Enemies/
      Enemy.cs               # mișcare pe traseu, viață, recompensă, daune bazei
      WaveSpawner.cs         # generează și lansează valurile
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
