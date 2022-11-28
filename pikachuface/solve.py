# pikachuface solution script
# idea:
#  - notice every pixel has its own mediaquery
#  - collect all viewport ranges for which every pixel is active from the media queries
#  - render every image possible in the ranges while deduplicating
from PIL import Image
import os
from multiprocessing import Pool, set_start_method
class Rect(object):
    def __init__(self) -> None:
        self.min_width = None
        self.max_width = None
        self.min_height = None
        self.max_height = None
    
    def __eq__(self, other: object) -> bool:
        return self.min_width == other.min_width and self.max_width == other.max_width and self.min_height == other.min_height and self.max_height == other.max_height

    def __hash__(self) -> int:
        return hash((self.min_width, self.max_width, self.min_height, self.max_height))

def dump_unique_images(r):
    global ranges
    global seen_images
    is_inside = lambda x, y, rule: (rule.min_width is None or x >= rule.min_width) and (rule.max_width is None or x <= rule.max_width) and (rule.min_height is None or y >= rule.min_height) and (rule.max_height is None or y <= rule.max_height)
    x = r.min_width
    y = r.min_height
    if x is None or y is None:
        return
    img = bytearray(100*100)
    for pixel, rules in ranges.items():
        for i in range(len(rules) - 1, -1, -1):
            rule = rules[i]
            if is_inside(x, y, rule):
                img[pixel] = 0xff
                del rules[i]
                break
    img = bytes(img)
    if img in seen_images:
        return
    seen_images.add(img)
    im = Image.frombytes('L', (100, 100), img)
    filename = f'img_{x}_{y}.png'
    im.save(filename)
    im.close()
    print(f'Saved {filename}.')

ranges = {}
seen_images = set()
if __name__ == '__main__':
    unique_ranges = set()
    with open('pikachuface.svg', 'r') as f:
        pixel_rules = []
        for line in f:
            if '@media all' in line:
                pixel_rules = []
                idx = line.index('@media all and ') + 15
                while True:
                    end_rule = line.find('))', idx)
                    rules = line[idx+1:end_rule+1]
                    idx = end_rule + 1
                    pixel_rule = Rect()
                    for rule in rules.split(' and '):
                        rule = rule[1:-1]
                        dimension, px = rule.split(': ')
                        val = int(px[:-2])
                        setattr(pixel_rule, dimension.replace('-', '_'), val)
                    unique_ranges.add(pixel_rule)
                    pixel_rules.append(pixel_rule)
                    idx = line.find('((', idx)
                    if idx == -1:
                        break
            elif '#pixel_' in line:
                pixel_start = line.index('#pixel_')+7
                pixel = line[pixel_start:line.index(' ', pixel_start)]
                ranges[int(pixel)] = pixel_rules
        print(f'Unique ranges: {len(unique_ranges)}')
        print(f'Pixels: {len(ranges)}')
    
    # fork to keep global variables
    set_start_method('fork')
    with Pool() as p:
        p.map(dump_unique_images, unique_ranges, len(unique_ranges)//os.cpu_count())