# Outlanding
Les renseignements fournis le sont a titre indicatif et ne sauraient engager la responsabilité des contributeurs et encore moins celle des propriétaires des champs.
## Download
| Fichier  | Format  | Lien | Commentaire | Gen. Auto. | Download depuis ![xcsoar](doc/images/xcsoar.png)|
|---|---|---|---|---|---|
| Guide des Aires de securite | cup | [guide_aires_securite.cup](https://planeur-net.github.io/outlanding/guide_aires_securite.cup) | | :pencil2: | |
| | cupx | [guide_aires_securite.cupx](https://planeur-net.github.io/outlanding/guide_aires_securite.cupx) | | :heavy_check_mark:
| | kml | [guide_aires_securite.kml](https://planeur-net.github.io/outlanding/guide_aires_securite.kml) | | :heavy_check_mark:
| | kmz | [guide_aires_securite.kmz](https://planeur-net.github.io/outlanding/guide_aires_securite.kmz) | | :heavy_check_mark:
| | _details.txt | [guide_aires_securite_details.txt](https://planeur-net.github.io/outlanding/guide_aires_securite_details.txt) | XCSoar: Description des images | :heavy_check_mark:
| | | [combined_guide+champs_details.txt](https://planeur-net.github.io/outlanding/combined_guide+champs_details.txt) | XCSoar: Guide + Champs des Alpes | :heavy_check_mark:
| | zip | [guide_aires_securite.xcsoar.zip](https://planeur-net.github.io/outlanding/guide_aires_securite.xcsoar.zip) | [XCSoar: Tous les fichiers nécessaires](doc/xcsoar_waypoint_details.md) |:heavy_check_mark:
| | | [combined_guide+champs.xcsoar.zip](https://planeur-net.github.io/outlanding/combined_guide+champs.xcsoar.zip) | XCSoar: Guide + Champs des Alpes | :heavy_check_mark:
||||
| Champs des Alpes | cup | [champs_des_alpes.cup](https://planeur-net.github.io/outlanding/champs_des_alpes.cup) | | :pencil2: |
| | cupx | [champs_des_alpes.cupx](https://planeur-net.github.io/outlanding/champs_des_alpes.cupx) | | :heavy_check_mark:
||||
| Cols des Alpes | cup | [cols_des_alpes.cup](https://planeur-net.github.io/outlanding/cols_des_alpes.cup) | | :pencil2: | :heavy_check_mark: |
| | cupx | [cols_des_alpes.cupx](https://planeur-net.github.io/outlanding/cols_des_alpes.cupx) | | :heavy_check_mark:
| | kml| [cols_des_alpes.kml](https://planeur-net.github.io/outlanding/cols_des_alpes.kml) | | :heavy_check_mark:
| | kmz| [cols_des_alpes.kmz](https://planeur-net.github.io/outlanding/cols_des_alpes.kmz) | | :heavy_check_mark:
||||
| Montagnes des Alpes | cup | [mountain_peaks_FR.cup](https://planeur-net.github.io/outlanding/mountain_peaks_FR.cup) | France | :pencil2: | |
| | |  [mountain_peaks_CH.cup](https://planeur-net.github.io/outlanding/mountain_peaks_CH.cup) | Suisse | :pencil2: | |
| | |  [mountain_peaks_IT.cup](https://planeur-net.github.io/outlanding/mountain_peaks_IT.cup) | Italie | :pencil2: | |
| | |  [mountain_peaks_FR_CH_IT.cup](https://planeur-net.github.io/outlanding/mountain_peaks_FR_CH_IT.cup) | France + Suisse + Italie | :heavy_check_mark:
| | |  [mountain_peaks_Pyrenees.cup](https://planeur-net.github.io/outlanding/mountain_peaks_CH.cup) | Pyrenees | :pencil2: |

### Mise a jour des fichiers sur Android
<table>
<tr>
<td width=80%>  

Une application Android [GithubDownloader](https://github.com/llauner/AirspaceDownloader) permet de mettre a jours les fichiers dans les bons repertoires pour XCSoar ou SeeYou Navigator en 1 click.  
 Le repo github: https://github.com/llauner/AirspaceDownloader  
 Le fichier .apk: [com.llcs.airspacedownloader.apk](https://github.com/llauner/AirspaceDownloader/releases)

</td>
<td>
<img src="https://github.com/llauner/AirspaceDownloader/raw/master/doc/images/screenshot_main_screen.jpg" alt="drawing" width="97" height=166/>
</td>
</tr>
</table>

## Fichiers de champs "vachables" pour les Alpes
| Fichier  | Commentaire  |
|---|---|
|Guide des Aires de securite| Reprend les aerodromes et champs définis dans le Guide des aires de sécurité dans les Alpes Edition 4.1 Peut cependant contenir des differences liées aux corrections apportées par les contributeurs (altitude, position exacte, ...). [Fiabilité: bonne].
| Champs des Alpes | Autres champs définis par des contributeurs. [Fiabilité: moyenne]
|Cols des Alpes | Cols intéressants dans les Alpes
|mountain_peaks_ | Noms des sommets en France, Suisse, Italie, Autriche et Pyrénées. :warning: Le fichier FR est en cours de simplification (densité des noms, noms vélivoles, ...). Source: [blog.zirbitzkogel.at](http://zirbitzkogel.at/blog/en/2021/06/04/peaks-mountain-peaks-in-seeyou-cup-format-for-xcsoar/)


### Les formats suivants sont maintenus:
- .cup : Le fichier source. [SeeYou cup file format](./doc/SeeYou_CUP_file_format.pdf)
- .cupx : Fichier généré automatiquement a partir du .cup et du repertoire /Pics [SeeYou cupx file format](./doc/SeeYou_cupx_file_format.md)
- .kml + .kmz : Format Google Earth. Fichier générés automatiquement a partir du .cup
- [XCSoar _details.txt](#fichier-_detailstxt-pour-xcsoar): Fichier généré automatiquement
- [guide_aires_securite.xcsoar.zip](#fichier-_detailstxt-pour-xcsoar): Fichier généré automatiquement. Contient les fichiers a copier dans le repertoire XCSoar pour le Guide des Aires de Sécurité.  
- [combined_guide+champs.xcsoar.zip](#fichier-_detailstxt-pour-xcsoar): Fichier généré automatiquement. Contient les fichiers a copier dans le repertoire XCSoar pour le Guide des Aires de Securite + Champs des Alpes. 
  
[![Vue Google map du fichier .kmz](doc/images/kmz_googlemap_view_small.png)](doc/images/kmz_googlemap_view.png)

### Fichier _details.txt pour XCSoar
Fichier généré automatiquement:  
Le fichier *guide_aires_securite_details.txt* ainsi que le repertoire *Pics* permettent d'associer une image a un point (champ, aerodrome, ...).  
Voir l'aide a l'installation: [xcsoar_waypoint_details.md](doc/xcsoar_waypoint_details.md)    
  
 ![Site Files](doc/images/Screenshot_XCSoar_wp_details.png)

### Guide de nommage
La version du guide de reference est indiquée dans la 2e ligne du fichier cup, dans la colonne code.
```
"version=",4.1,,,,,,,,,,"c0007a7 2023-12-15T13:59:33+01:00",,
```
#<num_page> <nom_aero>: Aerodrome. Le code est le code OACI de l'aerodrome  
```
"#60 LFLG Grenoble Versoud",LFLG,FR,4513.150N,00550.950E,220.0m,5,40,890.0m,,"121.000",,,"N090E005LFLG.jpg"
```

<numero_champs> <nom_champs>: Le numero du champs correspond au numero dans le rectangle colore en haut a gauche (ou droite) de la page du champs dans le guide. 
```
"213 Aups",V13,FR,4337.517N,00610.983E,450.0m,3,0,300.0m,,,"Zone cultures",,
```

#### Commentaires
Les commentaires proviennent généralement du guide. A la fin de chaque commentaire est ajoute un "tag" qui permet d’identifier rapidement la difficulté associée.  
Les "tag" sont:
| Tag  | Commentaire  | Couleur dans le guide | kmz icon|
|---|---|---|---|
|  {aerodrome} | Aerodrome ou aéroport  | blanc | ![](doc/images/runway.png)|
|  {terrain} | Terrain balise (prive) ou piste ULM | blanc | ![](doc/images/windsock.png)|
|  {altiport} | Altiport | blanc | ![](doc/images/altiport.png)|
|  {velisurface} | Velisurface |  |![](doc/images/velisurface.png) |
|  {facile} | Champ sans difficulté particulière. Adapte a tous les types de machines  | Vert | ![](doc/images/marker_green.png)|
|  {normal} | Champ avec une ou plusieurs difficultés. (Un seul sens, taille réduite, ...) | Orange | ![](doc/images/marker_orange.png)|
|  {difficile} | Champ difficile. Pas forcement utilisable avec les grandes plumes, casse possible, ...  | Rouge | ![](doc/images/marker_red.png)| 
|  {tres_difficile} | Champ tres difficile, a utiliser qu'en ultime recours, casse probable...  | Noir | ![](doc/images/marker_black.png)|
|||||
|  {col} | Col de montagne  |  | ![](doc/images/mountain_pass.png)|

## Copyrights - Droits - Remerciements
Images fournies et utilisées avec l'autorisation de Jérémie Badaroux.  
Fichiers initiaux de Moutain Peaks files par: Dr.-Ing. Stefan V. Baumgartner: [blog.zirbitzkogel.at](http://zirbitzkogel.at/blog/en/2021/06/04/peaks-mountain-peaks-in-seeyou-cup-format-for-xcsoar/)