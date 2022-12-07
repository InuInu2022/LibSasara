@rem 簡単につかえるようにするバッチファイル
@rem ファイルの中身を書き換えてダブルクリックするだけで使えます

echo "SongToTalk ボイパを簡単に作る簡易ツールです"
echo "EasySongToTalk.batの中身を書き換えて使ってください"

@rem 【書き換えてOK】ソングトラックのあるccs/ccstへのパス
@set SRC="./file/kaeru.ccs"
@rem 【書き換えてOK】上書きするccsへのパス
@set DIST="./file/dist.ccs"
@rem 【書き換えてOK】ボイパさせるキャラ名。持ってるキャラ名にしてね。
@set CAST="さとうささら"
@rem 【書き換えてOK】CeVIO_AI か CeVIO_CSか
@set TTS="CeVIO_AI"

call SongToTalk.exe -s %SRC% -d %DIST% -c %CAST% -tts %TTS%

echo "SongToTalk 生成が終了しました！"
pause