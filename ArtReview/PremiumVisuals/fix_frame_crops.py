from PIL import Image
from pathlib import Path
r=Path('B:/Soul-Of-Dead-War');im=Image.open(r/'Assets/UI/Extra Ui Images/Sprite Sources/Frames-Source.png')
boxes={'Panel':(20,40,510,510),'TitleBanner':(520,110,1015,350),'Button':(15,680,570,865),'CardFrame':(600,510,985,960),'SliderTrack':(15,1170,575,1280),'SelectionFrame':(600,960,995,1475)}
for name,box in boxes.items():im.crop(box).save(r/f'Assets/UI/Premium/Frames/{name}.png')
