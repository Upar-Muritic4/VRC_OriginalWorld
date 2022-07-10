# README #

This README would normally document whatever steps are necessary to get your application up and running.

### What is this repository for? ###

* Quick summary
* Version
* [Learn Markdown](https://bitbucket.org/tutorials/markdowndemo)

### How do I get set up? ###

* Summary of set up
* Configuration
* Dependencies
* Database configuration
* How to run tests
* Deployment instructions

### Contribution guidelines ###

* Writing tests
* Code review
* Other guidelines

### Who do I talk to? ###

* Repo owner or admin
* Other community or team contact

### ブランチ策定 ###

　[master] メインブランチ
　　→外部向けに纏めた内容

　[develop] 各々の検証環境ブランチ
　　→開発したプロジェクトをMasterにマージさせる
　　　(過去の開発ステップを一括管理用のブランチ)

　[feature] チケット管理ブランチ
　　→各開発チケット単位での開発を行う

---------------------------------------

◆ブランチルール
　[master]: master <- release
　　developが整った段階でmargeする。

　[develop]: develop <- feature¥xxx
　　featureが整った段階でmargeする。

　[feature]: ¥xxx 
　　feature¥番号で管理する。

---------------------------------------

◆ブランチ構成
　- master
　- develop
　- feature

---------------------------------------
以下、Git Repogitory作成で参考にしたサイト
https://www.sejuku.net/blog/83605

---------------------------------------

MainRepository「VirtualSpace」
xxx
--------------------------------------- 