from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
r=Path('B:/Soul-Of-Dead-War'); out=r/'ArtReview/PremiumVisuals';out.mkdir(exist_ok=True)
names=['SoulArcher','BoneMage','IronPaladin','FrostWitch','SkeletonRunner','ShieldBearer']
font=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',24)
canvas=Image.new('RGB',(1200,850),(9,18,35));d=ImageDraw.Draw(canvas)
for i,name in enumerate(names):
 x=(i%3)*400;y=(i//3)*425
 d.text((x+20,y+12),name,font=font,fill=(238,207,140))
 for j,action in enumerate(['Walk' if i>=4 else 'Idle','Attack','Death']):
  im=Image.open(r/f'Assets/UI/AnimationSheets/{name}/{action}_02.png').convert('RGBA');im.thumbnail((124,300));canvas.paste(im,(x+j*128+8,y+70),im);d.text((x+j*128+12,y+370),action,font=font,fill=(91,214,249))
canvas.save(out/'Character-Animation-Catalog.jpg',quality=95)
frames=[]
for t in range(18):
 im=Image.new('RGB',(1200,500),(9,18,35));d=ImageDraw.Draw(im)
 for i,name in enumerate(names):
  action=('Walk' if i>=4 else 'Idle') if t<6 else ('Attack' if t<12 else 'Death')
  sprite=Image.open(r/f'Assets/UI/AnimationSheets/{name}/{action}_{t%6:02}.png').convert('RGBA');sprite.thumbnail((195,310));im.paste(sprite,(i*200,100),sprite);d.text((i*200+8,30),name,font=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',18),fill='white');d.text((i*200+30,420),action,font=font,fill=(91,214,249))
 frames.append(im)
frames[0].save(out/'Character-Animations.gif',save_all=True,append_images=frames[1:],duration=120,loop=0)
