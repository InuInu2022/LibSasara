@rem �ȒP�ɂ�����悤�ɂ���o�b�`�t�@�C��
@rem �t�@�C���̒��g�����������ă_�u���N���b�N���邾���Ŏg���܂�

echo "SongToTalk �{�C�p���ȒP�ɍ��ȈՃc�[���ł�"
echo "EasySongToTalk.bat�̒��g�����������Ďg���Ă�������"

@rem �y����������OK�z�\���O�g���b�N�̂���ccs/ccst�ւ̃p�X
@set SRC="./file/kaeru.ccs"
@rem �y����������OK�z�㏑������ccs�ւ̃p�X
@set DIST="./file/dist.ccs"
@rem �y����������OK�z�{�C�p������L�������B�����Ă�L�������ɂ��ĂˁB
@set CAST="���Ƃ�������"
@rem �y����������OK�zCeVIO_AI �� CeVIO_CS��
@set TTS="CeVIO_AI"

call SongToTalk.exe -s %SRC% -d %DIST% -c %CAST% -tts %TTS%

echo "SongToTalk �������I�����܂����I"
pause