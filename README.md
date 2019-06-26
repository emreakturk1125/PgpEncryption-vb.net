# PgpEncryption

PGP ENCRYPTION NEDİR
----------------------

PGP Encryption, bir e-mail ve dosya şifreleme protokolüdür. IDEA, RSA, DSA, MD5, SHA-1 gibi şifreleme algoritmalarını bünyesinde barındırır. PGP’nin amacı sadece dosya ve e-maillerin korunmasıdır. 

PGP ENCRYPTION KULLANIMI
-----------------------------

1.) Tools kullanarak, 

2.) Herhangibir programlama diliyle,

-----------------------------------

1.) Tools Kullanarak

  a.) https://www.igolder.com/PGP/generate-key/   ->  sitesinden Public Key & Private Key oluşturulabilir

  b.) gpg4win.exe kurulumu yapılarak(Sitesinde ki en son versiyonda hata çıkabilir. Daha eski versiyonları deneyebilirsin)
masaüstüne 2 simge gelir GPA ve Kleopatra;

GPA 
------
ilk çalıştırıldığında Generate Key işlemi yapılır
ve rastgele girilecek Email ve Şifre ile Public & Private Key'ler oluşturulur
(Şifre : Encrypt ve Decrypt işlemlerinde lazım olacak)

Encrypt
---------

Eğer karşılıklı olarak Şifrelenmiş mesajlar gönderilecek ise;

Her iki tarafında (Private Key & Public Key) sahip olması gerekiyor.
Şifrelerken de, karşı taraf bizim public key’ imize göre şifrelemesi gerekiyor. 
Aynı şekilde bizde karşı tarafın Public key’ ine göre şifrelememiz gerekiyor.
Bize vermeleri gereken bilgiler (Public Key, Şifrelenmiş mesaj)

Eğer bütün encrypt ve decrypt işlemleri,  karşı tarafın göndereceği  Key’ lere göre olacak ise ,
Bize vermeleri gereken bilgiler (Public Key, Private Key, PassPhrase) 

Kullanılacak kütüphaneler
-------------------------

1.) Encryption işlemleri için BouncyCastle.Crypto.dll  (Open Source) ( http://www.bouncycastle.org/ ) 

2.) DidiSoft.BouncyCastle.dll  (Licence) -> Daha az kod ile istenilen şifreleme işlemleri yapılabilir


Proje hakkında
----------------

-> Visual basic console uygulamasıdır. Encrypt,Decrypt işlemleri ve Excel'e aktarma işlemleri mevcuttur

-> BouncyCastle.Crypto.dll  ve  DidiSoft.BouncyCastle.dll  olmak üzere proje içinde iki ayrı kütüphane kullanılarak, iki ayrı console uygulaması yapılmıştır 

-> Projeyi çalıştırabilmek için; https://www.igolder.com/PGP/generate-key/   ->  sitesinden Public Key & Private Key oluşturduktan sonra kod içindeki, dosyaların bulunacağı, dosya yollarını kendi local path' inize göre değiştirmelisiniz 

