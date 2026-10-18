from pathlib import Path
import subprocess
import imageio_ffmpeg

p=Path(__file__).resolve().parent
ff=imageio_ffmpeg.get_ffmpeg_exe()
subprocess.run([ff,'-y','-i',str(p/'Gameplay-Animated-Mockup.webm'),'-an','-c:v','libx264','-preset','fast','-crf','19','-pix_fmt','yuv420p','-movflags','+faststart',str(p/'Gameplay-Animated-Mockup-1080x1920.mp4')],check=True,capture_output=True)
subprocess.run([ff,'-y','-i',str(p/'Gameplay-Animated-Mockup-1080x1920.mp4'),'-vf','fps=10,scale=360:640:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=128[p];[s1][p]paletteuse=dither=bayer:bayer_scale=3','-loop','0',str(p/'Gameplay-Animated-Preview.gif')],check=True,capture_output=True)
result=subprocess.run([ff,'-i',str(p/'Gameplay-Animated-Mockup-1080x1920.mp4')],capture_output=True,text=True)
(p/'Video-Metadata.txt').write_text(result.stderr,encoding='utf-8')
print('Exported 1080x1920 MP4 and animated GIF.')
