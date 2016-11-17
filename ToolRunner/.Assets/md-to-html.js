var fs = require( 'fs');
var marked = require( 'marked');

console.log( 'reading file: ' + process.argv[2] )
var input = fs.readFileSync( process.argv[2], {encoding:'utf8'} );

console.log( 'creating html')
var output = marked.parse( input );

var header = '\
<!DOCTYPE html>\
	<head>\
    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/frameworks-c90d888b6f5151dadd198c29754c69062ad2d2b96ccd6484e1439cff77089623.css" media="all" rel="stylesheet" />\
    <link crossorigin="anonymous" href="https://assets-cdn.github.com/assets/github-5d574eecbac2894f972a707946d2a8354c1889e1a778c5648346ceb969c60660.css" media="all" rel="stylesheet" />\
	</head>\
	<body>\
		<div id="wiki-content">\
  		<div class="wrap">\
  			<div id="wiki-body" class="gollum-markdown-content instapaper_body">\
    			<div class="markdown-body">\
';

var footer = '\
	\
					</div>\
				</div>\
			</div>\
		</div>\
	</body>\
</html>\
;'


console.log( 'writing result: ' + process.argv[3] + ".html" )
fs.writeFileSync( process.argv[3] + ".html", header + output + footer, {encoding: 'utf8'} );

console.log( 'done' );

