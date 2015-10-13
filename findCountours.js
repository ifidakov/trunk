var cv = require('opencv');
	
	var lowThresh = 0;
	var highThresh = 100;
	var nIters = 1;
	var minArea = 500;
	
	var BLACK = [0, 0, 0]; // B, G, R
	var BLUE  = [255, 0, 0]; // B, G, R
	var RED   = [0, 0, 255]; // B, G, R
	var GREEN = [0, 255, 0]; // B, G, R
	var WHITE = [255, 255, 255]; // B, G, R
	var ORANGE = [55, 55, 255]; // B, G, R
	
	function getRatio(w, h) {
	  if (w < h) {
	    var t = w;
	    w = h;
	    h = w;
	  }
	  return w/h;
	
	}
	
	function getDarkness(img, x, y, w, h) {
	
	  x = x + Math.round(w/4);
	  y = y + Math.round(h/4);
	  h = h - Math.round(h/4)*2;
	  w = w - Math.round(w/4)*2;
	
	  var out = img.crop(x, y, w, h);
	
	  var darkness = 0;
	  var count = out.width() * out.height();
	  for (var xx = 0; xx < out.width(); xx++) {
	    for (var yy = 0; yy < out.height(); yy++) {
	      darkness += out.pixel(yy, xx);
	    }
	  }
	
	  darkness = 255 - darkness/count;
	  darknessPercent = (darkness * 100)/255;
	
	  return darknessPercent;
	
	}
	
	function findCountours(inm, inimg, outimg, x, y, w, h, type) {
	
	  outimg.rectangle([x, y], [w, h], ORANGE, 4);//cv.CV_FILLED);
	
	  var wrk = inimg.crop(x, y, w, h);
	  // wrk.save('out005-' + x + '_' + y + '_' + w + '_' + h + '.png');
	  // wrk.save('out006.png');
	
	  var hr = h/10;
	  var wr = w/10;
	  // console.log(hr, wr);
	
	  contours = wrk.findContours();
	  // for (i = 0; i < contours.size(); i++) {
	  for (var i = 0; i < contours.size(); i++) {
	
	    if (contours.area(i) < minArea) continue;
	    var arcLength = contours.arcLength(i, true);
	    contours.approxPolyDP(i, 0.01 * arcLength, true);
	    contours.convexHull(i, true);
	    arcLength = contours.arcLength(i, true);
	
	    switch(type) {
	      case 'box':
	        // if (contours.cornerCount(i) == 4)  {
	// console.log(1);
	          // var ctr = new cv.Contour();
	          // outimg.drawContour(contours, i, RED, cv.CV_FILLED);
	          // wrk.save('out005-' + x + '_' + y + '_' + w + '_' + h + '_' + i + '.png');
	
	// contours.point(i, 0) = { x: 10, y: 20};
	
	          var rect = contours.boundingRect(i);
	
	          darknessPercent = getDarkness(inm, rect.x + x, rect.y + y, rect.width, rect.height);
	
	          // if (darknessPercent > 20) {
	            var ratio = getRatio(rect.width, rect.height);
	            // console.log(ratio);
	            if ((ratio > 0.5) && (ratio < 2.5)) {
	              if (((w - (rect.x + rect.width)) > wr) &&
	                  ((h - (rect.y + rect.height)) > hr) &&
	                  (rect.x > wr) &&
	                  (rect.y > hr)) {
	                outimg.rectangle([rect.x + x, rect.y + y], [rect.width, rect.height], RED, -1);//cv.CV_FILLED);
	              }
	            }
	          // }
	
	        // }
	        break;
	      case 'bubble':
	        var cornersCount = contours.cornerCount(i);
	        if ((cornersCount > 4)) {
	
	          var rect = contours.boundingRect(i);
	
	          darknessPercent = getDarkness(inm, rect.x + x, rect.y + y, rect.width, rect.height);
	
	          var hierarchy = contours.hierarchy(i);
	          // if ((hierarchy[2] == -1) && (hierarchy[3] == -1)) {
	            // if (arcLength < 400) {
	              // if (Math.abs(rect.width - rect.height) < 20) {
	                if (darknessPercent > 40) {
	                  outimg.rectangle([rect.x + x, rect.y + y], [rect.width, rect.height], BLUE, -1);//cv.CV_FILLED);
	                }
	              // }
	              // outimg.rectangle([rect.x + x, rect.y + y], [rect.width, rect.height], GREEN);//cv.CV_FILLED);
	              for (var k = 1; k < cornersCount; k++) {
	                outimg.line([contours.point(i, k-1).x + x, contours.point(i, k-1).y + y], [contours.point(i, k).x + x, contours.point(i,k).y + y], GREEN);
	              }
	              outimg.line([contours.point(i, cornersCount-1).x + x, contours.point(i, cornersCount-1).y + y], [contours.point(i, 0).x + x, contours.point(i, 0).y + y], GREEN);
	            // }
	          // }
	        }
	    }
	        // break;
	  }
	
	}
	
	function recognize(inFile, outFile) {
	  console.log('Processing ' + inFile);
	  cv.readImage(inFile, function(err, inm) {
	
	    if (err) throw err;
	
	    var width = inm.width();
	    var height = inm.height();
	    var bw = width/5.12;
	    var bh = width/6.6;
	    var bbw = bw/5;
	    var bbh = bh/5;
	    var bbh2 = bbh;
	
	    if (width < 1 || height < 1) throw new Error('Image has no size');
	
	    // var out = new cv.Matrix(height, width);
	    var out = inm.copy();
	    // out.save('out001.png');
	    // var out = im.crop(0,0,200,200);
	
	    inm.convertGrayscale();
	    inm.save('out002.png');
	
	    var im = inm.copy();
	
	    // im_canny = im.crop(0,0,200,200);
	
	    // im = im.adaptiveThreshold(255, 0, 0, 51, 1);
	    im.save('out003.png');
	    im.canny(lowThresh, highThresh);
	    // im.save('out003-.png');
	    // im_canny.save('out2.png');
	
	    // im = im.adaptiveBilateralFilter(255, 0, 0, 51, 1);
	    im.dilate(nIters);
	    im.save('out004.png');
	    // im_canny.save('out2.png');
	
	    findCountours(inm, im, out, 0, 0, bw, bh, 'box');
	    findCountours(inm, im, out, width - bw, 0, bw, bh, 'box');
	    findCountours(inm, im, out, width - bw, height-bh, bw, bh, 'box');
	    findCountours(inm, im, out, 0, height - bh, bw, bh, 'box');
	
	    // findCountours(inm, im, out, bbw+200, bh-50, width-bbw*2- 400, height-3*bh + 80, 'bubble');
	    findCountours(inm, im, out, bbw, bh+bbh, width-bbw*2- 400, height-2*bh + 80, 'bubble');
	
	    out.save(outFile);
	
	  });
	}
	
	recognize('im3.png', 'scan-res.png');
	// recognize('scan2.png', 'scan2-res.png');
	// recognize('0539386bdb718f2bd7bca52f133e007f.png', '0539386bdb718f2bd7bca52f133e007f-res.png');
	
	
	// var fs = require('fs');
	// var path = require('path');
	// fs.readdir('./png/', function(err, list) {
	//   for(var i = 0; i < list.length; i++) {
	//     recognize('./png/' + list[i], './png-out/' + list[i]);
	//   }
	// });