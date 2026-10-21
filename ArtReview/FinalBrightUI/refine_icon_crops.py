from PIL import Image
from pathlib import Path
r=Path('B:/Soul-Of-Dead-War/Assets/UI/Final/Production');im=Image.open(r/'Sources/Icons-Atlas.png').convert('RGBA')
boxes={'Crystal':(0,15,442,465),'Pulse':(450,50,890,465),'Storm':(900,30,1310,460),'Barrier':(1350,25,1745,448),'Pause':(60,500,410,850),'Gem':(520,510,790,820),'Elephant':(819,460,1311,878),'Train':(1320,475,1774,880)}
for name,box in boxes.items():
 tile=im.crop(box);bb=tile.getchannel('A').point(lambda v:255 if v>40 else 0).getbbox()
 if bb:tile=tile.crop(bb)
 tile.save(r/'Icons'/(name+'.png'))
