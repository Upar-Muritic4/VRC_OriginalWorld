
● README

本アセットはiwaSync3と「仮想狐のデザイン工房」さんが配布している「Lura's Switch【SDK2／SDK3】」を組み合わせて使用する時のサンプルアセットです。
利用するためには別途「Lura's Switch【SDK2／SDK3】」のインポートが必要です。
以下より利用条件の確認の上、ダウンロード＆インポートをお願いします。

仮想狐のデザイン工房
Lura's Switch【SDK2／SDK3】
https://lura.booth.pm/items/1969082


● 使い方(かんたん)

1. サンプルシーンの「iwaSync3」などとスイッチをセットで自分のワールドのシーンへ移動させる

2. 位置などを調整する


● 使い方(むずかしめ)

1. プレハブをワールドのシーンに設置する
Switch_WithVideo    ：ビデオスイッチ
Object_WithSwitch   ：汎用スイッチ

2. 設置したオブジェクトの設定部分を探します
Switch_WithVideoの場合は展開した中の「Switch_Video」を選ぶ
Object_WithSwitchの場合は「Switch_Object」を選択する

3. Inspectorウィンドウで値を設定します

《「iwaSync3表示切替」の場合》
※Switch_WithVideoプレハブを使用
※Switch_WithVideo＞VideoSwitchObject内の「Switch_Video_Off」を無効化して、「Switch_Video_On」を有効化しておく
Target Object   ：Element 0に「iwaSync3」オブジェクトをセット

《「Playlist表示切替」の場合》
※Object_WithSwitchプレハブを使用
※Object_WithSwitch＞ObjectSwitchObject内の「Switch_Object_Off」を無効化して、「Switch_Object_On」を有効化しておく
Target Object   ：Element 0に「iwaSync3-Playlist」オブジェクトをセット

《「Queuelist表示切替」の場合》
※Object_WithSwitchプレハブを使用
※Object_WithSwitch＞ObjectSwitchObject内の「Switch_Object_Off」を無効化して、「Switch_Object_On」を有効化しておく
Target Object   ：Element 0に「iwaSync3-Queuelist」オブジェクトをセット
