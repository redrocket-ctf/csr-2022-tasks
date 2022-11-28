const fs = require('fs');
const { createCanvas, loadImage } = require('canvas');

const gridSize = 100;

function letterToPixels(index, letter) {
    const canvas = createCanvas(gridSize, gridSize);
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = 'rgba(255,255,255,1)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.font = gridSize + 'px Sans';
    ctx.fillStyle = 'rgba(255,0,0,1)';
    ctx.fillText(letter, gridSize / 4, gridSize * 3 / 4);

    ctx.font = gridSize/4 + 'px Sans';
    ctx.fillStyle = 'rgba(255,0,0,1)';
    ctx.fillText(''+index, 5, gridSize - 5);

    return {
        data: ctx.getImageData(0, 0, gridSize, gridSize).data,
        threshold: 130,
    };
}

async function memePicture(meme) {
    const image = await loadImage(meme['path']);
    const canvas = createCanvas(gridSize, gridSize);
    const ctx = canvas.getContext('2d');
    ctx.drawImage(image, 0, 0, gridSize, gridSize);

    return {
        data: ctx.getImageData(0, 0, gridSize, gridSize).data,
        threshold: meme['threshold'],
    };
}

let lastMeme = null;
function getRandomMeme(memes) {
    while (true) {
        const newMeme = memes[random(0, memes.length)];
        if (newMeme != lastMeme) {
            return lastMeme = newMeme;
        }
    }
}

async function getRanges(memes, message) {
    const ranges = [];
    const minMediaQueryWidth = 600;
    const minMediaQueryHeight = 400;
    const maxMediaQueryWidth = 1920;
    const maxMediaQueryHeight = 1080;
    const minStepWidth = 20;
    const maxStepWidth = 30;
    let current_x = minMediaQueryWidth;
    let current_y = minMediaQueryHeight;
    const encoded_memes = [];
    for (const meme of memes) {
        encoded_memes.push(await memePicture(meme));
    }

    // Generate all flag ranges
    const dot = (a, b) => a.map((x, i) => a[i] * b[i]).reduce((m, n) => m + n);
    const add = (a, b) => a.map((x, i) => a[i] + b[i]);
    const overlaps = (r1, r2) => !((r1['horizontal_max'] + 1) <= r2['horizontal_min'] || (r1['vertical_max'] + 1) <= r2['vertical_min'] || r1['horizontal_min'] >= (r2['horizontal_max'] + 1) || r1['vertical_min'] >= (r2['vertical_max'] + 1));
    for (let i = 0; i < message.length; i++) {
        // Make sure we don't overlap
        let end_x = 0;
        let end_y = 0;
        let new_range = undefined;
        while (true) {
            end_x = current_x + random(minStepWidth, maxStepWidth);
            end_y = current_y + random(minStepWidth, maxStepWidth);
            new_range = { horizontal_min: current_x, horizontal_max: end_x, vertical_min: current_y, vertical_max: end_y };
            if (ranges.every(r => !overlaps(r, new_range))) {
                // console.log('no overlap end', end_x, end_y)
                break;
            }
            // console.log('!!!overlap end', end_x, end_y)
        }
        new_range['imageData'] = letterToPixels(i, message[i]);
        ranges.push(new_range);
        console.log(`${message[i]}: ${current_x}-${end_x}/${current_y}-${end_y}`);
        while (true) {
            current_x = random(minMediaQueryWidth, maxMediaQueryWidth);
            current_y = random(minMediaQueryHeight, maxMediaQueryHeight);
            new_range = { horizontal_min: current_x, horizontal_max: current_x + maxStepWidth, vertical_min: current_y, vertical_max: current_y + maxStepWidth };
            if (ranges.every(r => !overlaps(r, new_range))) {
                // console.log('no overlap', current_x, current_y)
                break;
            }
            // console.log('!!!overlap', current_x, current_y)
        }
    }

    function is_inside(x, y) {
        for (const r of ranges) {
            if (x >= r['horizontal_min'] && x <= r['horizontal_max'] && y >= r['vertical_min'] && y <= r['vertical_max'])
                return r;
        }
        return false;
    }

    // Fill in the memes into the gaps between the letters
    let startX = 0;
    let startY = 0;
    let endX = 0;
    let endY = 0;
    let skipX = 0;
    let skipY = 0;
    for (let x = minMediaQueryWidth; x < maxMediaQueryWidth; x++) {
        for (let y = minMediaQueryHeight; y < maxMediaQueryHeight; y++) {
            if (is_inside(x, y)) {
                continue
            }
            
            startX = endX = x;
            startY = endY = y;
            skipX = random(minStepWidth, maxStepWidth);
            skipY = random(minStepWidth, maxStepWidth);
            while (skipX > 0 || skipY > 0) {
                if (skipX > 0)
                    endX++;
                // moved into a letter
                let xInside = false;
                let new_range = { horizontal_min: startX, horizontal_max: endX, vertical_min: startY, vertical_max: endY };
                if (!ranges.every(r => !overlaps(r, new_range))) {
                    xInside = true;
                    endX--;
                }

                if (endX >= maxMediaQueryWidth)
                    endX--;

                if (skipY > 0)
                    endY++;
                let yInside = false;
                new_range = { horizontal_min: startX, horizontal_max: endX, vertical_min: startY, vertical_max: endY };
                if (!ranges.every(r => !overlaps(r, new_range))) {
                    yInside = true;
                    endY--;
                }
                if (endY >= maxMediaQueryHeight)
                    endY--;
                if (xInside && yInside)
                    break;

                skipX--;
                skipY--;
            }

            // console.log(startX, startY, endX, endY);
            ranges.push({ horizontal_min: startX, horizontal_max: endX, vertical_min: startY, vertical_max: endY, imageData: getRandomMeme(encoded_memes) });
        }
    }

    // Fill in around the min and max viewport
    ranges.push({ horizontal_min: 0, horizontal_max: minMediaQueryWidth-1, vertical_min: 0, vertical_max: 0, imageData: ranges[0].imageData})
    ranges.push({ horizontal_min: 0, horizontal_max: 0, vertical_min: 0, vertical_max: minMediaQueryHeight-1, imageData: ranges[0].imageData})
    ranges.push({ horizontal_min: 0, horizontal_max: 0, vertical_min: maxMediaQueryHeight, vertical_max: 0, imageData: encoded_memes[0]})
    ranges.push({ horizontal_min: maxMediaQueryWidth, horizontal_max: 0, vertical_min: 0, vertical_max: 0, imageData: encoded_memes[0]})

    const euclid_distance = (x) => Math.hypot(x['horizontal_min'] - minMediaQueryWidth, x['vertical_min'] - minMediaQueryHeight);
    ranges.sort((a, b) => {
        return euclid_distance(a) - euclid_distance(b);
    });

    return ranges;
}

function random(from, to) {
    return Math.floor(from + Math.random() * (to - from));
}

async function main() {
    const fd = fs.openSync('pikachuface.svg', "w");
    let rects = '';
    const width = 20;
    const height = 20;
    const startX = 0;
    const startY = 0;
    Math.seed

    for (let y = 0; y < gridSize; y++) {
        for (let x = 0; x < gridSize; x++) {
            const posx = startX + x * width;
            const posy = startY + y * height;

            rects += `<rect
            id="pixel_${(x + (y * gridSize))}"
            width="${width}"
            height="${height}"
            x="${posx}"
            y="${posy}" 
        />`;
        }
    }

    const flag = 'CSR{b35t_us3_c4s3_f0r_m3d1aqu3r13s_1n_svg5?}';
    const memes = [
        {path: 'pikachu.jpg', threshold: 130},
        {path: 'doge.jpeg', threshold: 130},
        {path: '1.jpeg', threshold: 130},
        {path: '2.jpeg', threshold: 130},
        {path: '3.jpeg', threshold: 130},
        {path: '4.jpeg', threshold: 130},
        {path: '5.jpeg', threshold: 130},
        {path: '6.jpeg', threshold: 130},
        {path: '7.jpeg', threshold: 130},
        {path: '8.jpeg', threshold: 130},
        {path: '9.jpeg', threshold: 130},
        {path: '10.jpeg', threshold: 130},
    ]
    const ranges = await getRanges(memes, flag);

    const svg_header = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${gridSize * width} ${gridSize * height}">\n<style>`;
    fs.writeSync(fd, svg_header);

    let style = '';
    // console.log(JSON.stringify(letterToPixels('C')));

    for (let y = 0; y < gridSize; y++) {
        for (let x = 0; x < gridSize; x++) {
            const pixelRanges = [];
            for (const range of ranges) {
                const index = x + (gridSize * y);
                const imageData = range.imageData.data;
                const pixelAvg = (imageData[index * 4] + imageData[index * 4 + 1] + imageData[index * 4 + 2]) / 3;
                // * 4 for RGBA
                if (pixelAvg < range.imageData.threshold) {
                    pixelRanges.push(range);
                }
            }
            if (pixelRanges.length > 0) {
                const allranges = pixelRanges.map(range => {
                    const rules = [];
                    if (range.horizontal_max != 0)
                        rules.push(`(max-width: ${range.horizontal_max}px)`)
                    if (range.horizontal_min != 0)
                        rules.push(`(min-width: ${range.horizontal_min}px)`);
                    if (range.vertical_max != 0)
                        rules.push(`(max-height: ${range.vertical_max}px)`);
                    if (range.vertical_min != 0)
                        rules.push(`(min-height: ${range.vertical_min}px)`)
                    return `(${rules.join(' and ')})`;
                }).join(', ');
                
                fs.writeSync(fd, `@media all and ${allranges} {
                        #pixel_${(x + (y * gridSize))} {
                            fill: green;
                        }
                    }
                `);
                
            }
        }
    }

    fs.writeSync(fd, `</style>
${rects}
</svg>`);

fs.close(fd);
}

main();
