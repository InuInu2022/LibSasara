@rem �ȒP�ɂ�����悤�ɂ���o�b�`�t�@�C��
@rem �t�@�C���̒��g�����������ă_�u���N���b�N���邾���Ŏg���܂�

echo "SongToTalkVoiSona VoiSona Talk���̂킹�郋�[���ł�"
echo "EasySongToTalkVoiSona.bat�̒��g�����������Ďg���Ă�������"

@rem �y����������OK�z�\���O�g���b�N�̂���ccs/ccst�ւ̃p�X
@set SRC="./file/kaeru.ccs"
@rem �y����������OK�z�㏑������ttsprj�ւ̃p�X
@set DIST="./file/dist.ttsprj"
@rem �y����������OK�z�̂킹��L�������B�����Ă�L�������ɂ��ĂˁBEnglish name is OK!
@set CAST="�c���P"
@rem �y����������OK�z����䗦�i�L���X�g���Ƃɗv�ύX�j
@set EMOTIONS="[1.0,0.0,0.0,0.0]"

call SongToTalkVoiSona.exe -s %SRC% -e %DIST% -c %CAST% -er %EMOTIONS%

echo "SongToTalk �������I�����܂����I"
pause