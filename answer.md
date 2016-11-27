## デプロイ情報
#### サービスURL
[http://the-creation-bot.azurewebsites.net]()

#### リポジトリURL
[https://github.com/occar421/codecheck--chat-bot-2016winter]()

#### 使用言語
- C# (+ HTML, TypeScript, SCSS)

#### 主なライブラリ
- ASP.NET

####  ホスティングサービス
- Azure

## 独自コマンドの実装
#### 追加したコマンド
```
bot help
```
デフォルトコマンド一覧を取得する。

```
bot cmd [add|remove|list|help] [params...]
```
自作のコマンド（プログラム・NeoLuaスクリプト）を管理する。  

```
bot cmd add [program name] [argc] [program]

bot cmd add example 2 print("Hello " .. args[0] .. " and ")\n
println(args[1])\n
description = "This is Example. Just print passed 2 args."
```
自作のコマンドを追加。簡易的なテストを行い、エラーが発生した場合は中止する。  
実行時に引数を与えられ、`args[i]`のように取得する。  
`print`と`println`で、クライアントへのレスポンスを出せる。

```
bot cmd remove [program name]

bot cmd remove example
```
自作のコマンドを削除。

```
bot cmd list
```
自作のコマンドの一覧を表示。ここにあるものは`bot example`のように呼び出せる。

```
bot [program name] [args...]
```
上記のように、自作のコマンドを呼び出す。

#### コマンドの説明

## 創意工夫 & 作り込み
#### 作り込んだコマンド / 機能
コマンドは`bot cmd`しか実質は追加していないが、構想を立てたときからかなり考えて制作した。  
卒研でLinuxのBashを頻繁に使うようになったので、Botアプリでもそのような感覚でできたら、Botシステムとしては新鮮なのではないかと思った。  
スクリプトとしては（C#上なので）IronRubyやIronPythonを使うのもよかったかもしれないが、NeoLuaというLuaの派生版を使うことにした。これは、Luaを自分のプログラムで使ってみたかったのと、外部定義スクリプトとして採用されているケースをよく見かけたからである。  
無限ループや大量にメッセージを出し続ける迷惑なコマンドに対しては、ある程度のチェックを取り入れて登録はされないようにしているが、手の込んだイタズラを防ぐのは難しいのではないかと思う。

#### 創意工夫したポイント
上にも書いたが、自作のコマンドを登録できるようにした。これにより、SPRINT2016 Winterが終わっても機能が更新される（かもしれない）ので、飽きにくくなっていると思う。  
前回のBotプログラムの設計を継承しているが、細かい部分を改良して保守性がさらによくなるように書いたつもり。  
また、前回同様にWebページでのクライアントも実装した。今回はフォントと色にも凝り、結果がわかりやすくなったと思う。  