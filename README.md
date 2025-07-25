# Peak Mobile Case - Match 2 Oyunu

Bu proje, Peak tarafından verilen "Mobile Case" çalışması kapsamında geliştirilmiş bir Match-2 oyunudur. Proje, Unity oyun motorunun **2021.3.18f1** versiyonu kullanılarak C# dili ile geliştirilmiştir.

## 🚀 Genel Bakış

Oyuncuların temel amacı, aynı renkteki en az iki küpü eşleştirerek patlatmak ve bölüm hedeflerini (Goal) tamamlamaktır. Proje, referans videoda belirtilen tüm temel mekanikleri, UI gereksinimlerini ve özel obje (roket, balon, ördek) davranışlarını içermektedir.

## ✨ Özellikler

- **Dinamik Grid Yapısı**: Grid boyutu (genişlik ve yükseklik) `GameConfig` dosyası üzerinden kolayca değiştirilebilir ve dikdörtgen yapıyı destekler.
- **Değiştirilebilir Seviye Ayarları**: Her seviye için hamle sayısı, başlangıç dizilimi ve hedefler (toplanacak küp, balon veya ördek sayısı) `GameConfig` üzerinden ayarlanabilir.
- **Duyarlı UI (Responsive UI)**: UI elementleri, farklı ekran çözünürlüklerine uyum sağlayacak şekilde dinamik olarak konumlandırılmıştır.
- **Temel Oyun Mekanikleri**:
  - **Match**: Yan yana duran aynı renkteki en az iki küpün eşleşmesi.
  - **Fill & Fall**: Patlayan küplerin yerine üstteki küplerin düşmesi (Fill) ve boş kalan yerlere yukarıdan yeni küplerin gelmesi (Fall).
- **Özel Objeler ve Mekanikler**:
  - **🚀 Roket**: 5 veya daha fazla küp eşleştirildiğinde, tıklandığı konumda rastgele yatay veya dikey bir roket oluşur. Roket aktive edildiğinde bulunduğu satır veya sütundaki tüm objeleri yok eder.
  - **🎈 Balon**: Küplerle birlikte düşen bir engeldir. Yanındaki bir eşleşme patlatıldığında yok olur. Seviye hedeflerine eklenebilir.
  - **🦆 Ördek**: Küplerle birlikte düşebilen bir objedir. Grid'in en alt satırına ulaştığında toplanır ve yok olur. Seviye hedeflerine eklenebilir.
- **Efektler ve Animasyonlar**:
  - Küp patlama, roket aktivasyonu ve hedef toplama anları için **parçacık (particle) ve ses efektleri** mevcuttur.
  - Toplanan hedeflerin UI'daki hedefe doğru uçma animasyonu.
  - Roketin hareket animasyonu.
- **Dinamik Çerçeve**: Grid'in etrafındaki çerçeve, grid boyutuna göre dinamik olarak ölçeklenir.

## 🔧 Teknik Detaylar ve Mimari

Proje, genişletilebilir ve temiz bir kod yapısı hedefiyle geliştirilmiştir. Ana bileşenler şunlardır:

- **GameManager**: Oyunun genel durumunu (hamle sayısı, hedefler, oyunun aktif olup olmadığı) yönetir. Diğer tüm manager'ları koordine eder.
- **GridManager**: Grid oluşturma, küp yerleştirme, eşleşme bulma, yerçekimi uygulama ve özel obje (roket, balon, ördek) mekaniklerini yönetir.
- **UIManager**: Hamle sayısı ve hedefler gibi UI elementlerini günceller, oyun sonu ekranlarını (kazanma/kaybetme) gösterir.
- **AudioManager**: Oyun içi ses efektlerini (patlama, toplanma vb.) çalar.
- **EffectsManager**: Patlama ve toplanma gibi anlar için parçacık efektlerini oluşturur ve yönetir.
- **GameConfig (ScriptableObject)**: Oyunun tüm ayarlarının (grid boyutu, hamle sayısı, hedefler, sesler, görseller vb.) tutulduğu merkezi yapı. Bu dosya sayesinde koda dokunmadan oyunun dengesi ve seviye tasarımları kolayca değiştirilebilir.
- **Geliştirilebilirlik**: Kod mimarisi, yeni mekaniklerin (örneğin yeni bir özel küp veya engel) sisteme kolayca entegre edilebilmesine olanak tanıyacak şekilde tasarlanmıştır. `GridCell` yapısı, hücrelere farklı obje türlerinin (küp, roket, balon, ördek) eklenebilmesini destekler.

## ⚙️ Kurulum ve Çalıştırma

1.  Projeyi Unity Hub üzerinden açın. Projenin **Unity 2021.3.18f1** versiyonu ile geliştirildiğinden emin olun.
2.  `Assets/Scenes` klasöründeki ana oyun sahnesini açın.
3.  Unity Editor üzerinden **Play** butonuna basarak oyunu test edebilirsiniz.
4.  **Build Settings** (File > Build Settings) menüsünden **iOS** veya **Android** platformunu seçerek projenin çıktısını alabilirsiniz.

## 🎮 Seviye Ayarlarını Değiştirme

Tüm oyun ve seviye ayarları `Assets/Resources` klasöründe bulunan **GameConfig** asset'i üzerinden değiştirilebilir:

- **Grid Settings**: Grid genişliği ve yüksekliği.
- **Gameplay Settings**: Hamle sayısı, roket için gereken eşleşme sayısı, seviyedeki balon ve ördek sayısı.
- **Level Goals**: Seviye hedeflerini (hangi renkten kaç küp, kaç balon, kaç ördek toplanacağı) buradan belirleyebilirsiniz.
- **Sprites, AudioClips, vb.**: Oyun içi tüm görseller ve sesler bu dosya üzerinden atanabilir.
