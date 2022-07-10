
● README

本アセットはiwaSync3と「生チョコ教団」さんが配布している「ヨドコロちゃんハプティックコントローラー」を組み合わせて使用する時のサンプルアセットです。
利用するためには別途「ヨドコロちゃんハプティックコントローラー」のインポートが必要です。
以下より利用条件の確認の上、ダウンロード＆インポートをお願いします。

生チョコ教団
ヨドコロちゃんハプティックコントローラー
https://yodokoro.booth.pm/items/2458051


● 使い方(かんたん)

1. サンプルシーンの「iwaSync3」などとスイッチをセットで自分のワールドのシーンへ移動させる

2. 位置などを調整する


● 使い方(むずかしめ)

1. 「Yodo_HapticHandProvider」プレハブをシーンに設置する
※シーンに1つあれば良い

2. プレハブをワールドのシーンに設置する
YodoHapticSlider    ：スライダタイプ
YodoHapticSwitches  ：スイッチタイプ

3. 設置したオブジェクトの設定部分を探します
YodoHapticSliderの場合は展開した中の「Slider_Pickup」を選ぶ
YodoHapticSwitchesの場合は「YodoHapticSwitches」を選択する

4. Inspectorウィンドウで値を設定します
設定する値は操作したいものによって変わりますので以下を参考に設定して下さい

《「音量」の場合》
※YodoHapticSliderプレハブを使用
Yodo_Haptic Slider Value    ：0.184
Yodo_Target Udon            ：Sizeに「1」を入力、Element 0に「Udon (VideoController)」オブジェクトをセット
                                ※「iwaSync3」＞「iwaSync3-Controller」＞「Udon (VideoController)」にあります
Yodo_Target Variable Name   ：Sizeに「1」を入力、「defaultVolume」を入力
Yodo_Min Knob X             ：-0.195

《「画面の明るさ」の場合》
※YodoHapticSliderプレハブを使用
Yodo_Haptic Slider Value    ：1
Yodo_Target Udon            ：Sizeに「1」を入力、Element 0に「Udon (Screen)」オブジェクトをセット
                                ※「iwaSync3」＞「iwaSync3-Screen」＞「Udon (Screen)」にあります
Yodo_Target Variable Name   ：Sizeに「1」を入力、「defaultEmissiveBoost」を入力
Yodo_Min Knob X             ：-0.195

《「iwaSync3表示切替」の場合》
※YodoHapticSwitchesプレハブを使用
※YodoHapticSwitches内の「Base」と「Video_ON」オブジェクト以外を無効化しておく
Yodo_Switch Off Object      ：YodoHapticSwitches内の「Video_OFF」オブジェクトをセット
Yodo_Switch On Object       ：YodoHapticSwitches内の「Video_ON」オブジェクトをセット
Yodo_Toggle Target Object   ：Sizeに「1」を入力、Element 0に「iwaSync3」オブジェクトをセット
Yodo_Default Status         ：✓

《「Playlist表示切替」の場合》
※YodoHapticSwitchesプレハブを使用
※YodoHapticSwitches内の「Base」と「GeneralObject_ON」オブジェクト以外を無効化しておく
Yodo_Switch Off Object      ：YodoHapticSwitches内の「GeneralObject_OFF」オブジェクトをセット
Yodo_Switch On Object       ：YodoHapticSwitches内の「GeneralObject_ON」オブジェクトをセット
Yodo_Toggle Target Object   ：Sizeに「1」を入力、Element 0に「iwaSync3-Playlist」オブジェクトをセット
Yodo_Default Status         ：✓

《「Queuelist表示切替」の場合》
※YodoHapticSwitchesプレハブを使用
※YodoHapticSwitches内の「Base」と「GeneralObject_ON」オブジェクト以外を無効化しておく
Yodo_Switch Off Object      ：YodoHapticSwitches内の「GeneralObject_OFF」オブジェクトをセット
Yodo_Switch On Object       ：YodoHapticSwitches内の「GeneralObject_ON」オブジェクトをセット
Yodo_Toggle Target Object   ：Sizeに「1」を入力、Element 0に「iwaSync3-Queuelist」オブジェクトをセット
Yodo_Default Status         ：✓
