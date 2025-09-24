@echo off
echo Creating notification sound files for Pomodoro Timer...

REM Create Assets directory if it doesn't exist
if not exist "Assets" mkdir Assets

echo Creating placeholder WAV files...
echo. > Assets\work_complete.wav
echo. > Assets\break_complete.wav
echo. > Assets\session_start.wav
echo. > Assets\notification.wav

echo.
echo Sound files created successfully in Assets folder:
echo - work_complete.wav    (placeholder for work completion sound)
echo - break_complete.wav   (placeholder for break completion sound)  
echo - session_start.wav    (placeholder for session start sound)
echo - notification.wav     (placeholder for general notifications)
echo.
echo The application will use system sounds as fallback.
echo To add custom sounds, replace these placeholder files with actual WAV files.
echo.
pause
