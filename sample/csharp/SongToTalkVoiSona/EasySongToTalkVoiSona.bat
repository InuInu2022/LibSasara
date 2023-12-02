@rem 簡単につかえるようにするバッチファイル
@rem ファイルの中身を書き換えてダブルクリックするだけで使えます

echo "SongToTalkVoiSona VoiSona Talkを歌わせるルールです"
echo "EasySongToTalkVoiSona.batの中身を書き換えて使ってください"

@rem 【書き換えてOK】ソングトラックのあるccs/ccstへのパス
@set SRC="./file/kaeru.ccs"
@rem 【書き換えてOK】上書きするttsprjへのパス
@set DIST="./file/dist.ttsprj"
@rem 【書き換えてOK】歌わせるキャラ名。持ってるキャラ名にしてね。English name is OK!
@set CAST="田中傘"
@rem 【書き換えてOK】感情比率（キャストごとに要変更）
@set EMOTIONS="[1.0,0.0,0.0,0.0]"

call SongToTalkVoiSona.exe -s %SRC% -e %DIST% -c %CAST% -er %EMOTIONS%

echo "SongToTalk 生成が終了しました！"
pause