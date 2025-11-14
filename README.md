para instalar e servicio se debe hacer un "publish" a una carpeta creada previamente como por ejemplo "Precompiled", posteriormente ejecutar el comando 
sc.exe create "NombreServicio" binPath= "Ruta\Precompiled\FtpRxtoJsonService.exe" DisplayName= "NombreServicio" start= auto
Donde NombreServicio es el nombre con el que se mostrará en el visor de servicios, y la ruta lleve hacia el ejecutable dentro de la carpeta donde se hizo el "publish"
En este proyecto ya existe la carpeta precompiled con el ejecutable
