# outlanding
## Fichiers de champs "vachables" pour les Alpes
Les fichiers suivants sont maintenus:
- .cup : representant les champs et aerodromes du Guide des aires de sécurité dans les Alpes Edition 4.1
- .cupx : fichier généré automatiquement a partir du .cup et du repertoire /Pics [See: SeeYou cupx file format](./SeeYou_cupx_file_format.md)

### Guide de nommage
La version du guide de reference est indiquee dans la 2e ligne du fichier cup, dans la colonne code.
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