# AsyncTextureReader
![Unityバージョン](https://img.shields.io/badge/Unity-2018.4.23f1-blue) ![.NETバージョン](https://img.shields.io/badge/.NET-4.x-blueviolet) ![ライセンス](https://img.shields.io/github/license/Nox-Lib/AsyncTextureReader)

## Overview
ビルドインアセットでないテクスチャを非同期＋アンマネージドメモリを使用して読み込みます。

## Description
Temporaryなどから画像を読み込む時にTexture2D.LoadImageを使用すると、負荷が気になることがあります。
~~~cs
string filePath = Application.persistentDataPath + "/image/hoge.png";
byte[] byteData = File.ReadAllBytes(filePath);
Texture2D texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
texture.LoadImage(byteData);
~~~
これは読み込む画像のサイズや数によりますが、以下のことが考えられます。

1. 画像データを読み込んでテクスチャが生成されるまでの間、処理を止めてしまうこと。
2. 画像データがbyte配列として一時的にメモリ確保されてしまうこと。このbyte配列は決して小さくないため、ヒープ領域の拡張が行われ、GCのパフォーマンスが悪くなる可能性が考えられます。
3. Texture2D.LoadImageはテクスチャをフルカラー（RGBA32）で読み込むため、メモリに優しくないこと。

AsyncTextureReaderでは、これらに以下のようにアプローチします。

1. 非同期読み込み
2. 不要なメモリ管理の負荷を回避するためにアンマネージドメモリで処理する
3. 圧縮されたテクスチャのバイナリデータを扱う（PVRTC、ETC2、ASTC、など）

## Demo
サンプルプロジェクトでは、1000x1000のテクスチャを100回読み込んで表示を繰り返すことで、比較を確認できます。
<br>
また、このデモはAndroid端末のPixel3で実行した時のものです。

<img src="https://github.com/Nox-Lib/AsyncTextureReader/blob/master/Demo/example.png"  width="700" height="700">

| |テクスチャ<br>フォーマット|FPS|1フレームあたりの<br>負荷|全体の<br>処理時間|ファイルサイズ|メモリサイズ|
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
|Texture2D.LoadImage|RGBA32|40|16ms以上|2.6秒|692KB|7.6MB|
|AsyncTextureReader|ETC2_RGBA8|60|1ms 〜 2ms	|3.3秒|977KB|1.9MB|
|AssetBundle|ETC2_RGBA8|60|1ms 〜 2ms|4.8秒|175KB|1.9MB|

※ ファイルサイズ、メモリサイズはテクスチャ１枚あたりです。

## Requirement
- Unity 2018.4.x 以上で動作確認（iOS、Android）
- .NET 4.x

## Usage
#### □使い方
読み込みたいテクスチャの情報を用意し、AsyncTextureReader.Readメソッドにて読み込みを行います。
~~~cs
// using AsyncReadTexture;

AsyncTextureReader.TextureData textureData = new AsyncTextureReader.TextureData {
    filePath = Application.persistentDataPath + "/image/hoge.png",
    format = TextureFormat.ETC2_RGBA8,
    width = 1000,
    height = 1000
};

AsyncTextureReader.Read(
    textureData,
    texture => {
        rawImage.texture = texture;
        rawImage.SetNativeSize();
    }
);
~~~

#### □圧縮テクスチャのバイナリデータを用意する例
AsyncTextureReaderではTexture2D.LoadImageのようにPNG画像などは扱えず、圧縮されたテクスチャのバイナリデータを読み込みます。

1. 読み込みたいテクスチャのインポーター設定で、任意のテクスチャフォーマットを指定する。
2. Texture2D.GetRawTextureDataメソッドでバイナリデータを取得し、File.WriteAllBytesメソッドにて書き出す。

<img src="https://github.com/Nox-Lib/AsyncTextureReader/blob/master/Demo/texture_importer.png" width="467" hieght="137">

~~~cs
File.WriteAllBytes(
    Application.persistentDataPath + "/image/hoge.png",
    texture2dAsset.GetRawTextureData()
);
~~~

## Licence
[MIT](https://github.com/Nox-Lib/AsyncTextureReader/blob/master/LICENSE)

## Author
[Nox-Lib](https://github.com/Nox-Lib)
