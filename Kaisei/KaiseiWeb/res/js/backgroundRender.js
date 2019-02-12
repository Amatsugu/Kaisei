var renderer;
var scene;
var camera;
var particles = [];
var velocities = [];
var frustumSize = 60;
var speed = .5;
var aspect;

var feild = [];
var feildLines = [];
var feildScale = 1;
var lineMaterial;

$(document).ready(function(){
	scene = new THREE.Scene();
	noise.seed(Date.now());
	lineMaterial = new THREE.LineBasicMaterial({
		 color: 0xff0064,
		 linewidth: 1
		});
	scene.background = new THREE.Color( 0x800032);
	aspect = window.innerWidth /  window.innerHeight;
	camera = new THREE.OrthographicCamera( 0, frustumSize * aspect, frustumSize, 0, 0.1, 1000 );
	renderer = new THREE.WebGLRenderer({
		antialias: true,
	});
	renderer.setSize(window.innerWidth, window.innerHeight);
	$("#background").append(renderer.domElement);

	/*var sprite = new THREE.TextureLoader().load('res/js/sprites/Fire Spark.png');
	var material = new THREE.SpriteMaterial({
		color : 0xff0064,
		map: sprite,
		alphaTest: 0.5,
		transparent: true
	});*/
	// var m = new THREE.MeshBasicMaterial({ color : 0xff0064 });
	// var s = new THREE.BoxGeometry(1, 1, 1);
	/*for(var i = 0; i < 100; i++)
	{
		particles[i] = new THREE.Sprite(material);
		var x = (Math.random() - .5);
		var y = (Math.random() - .5);
		// scene.add(particles[i]);
		particles[i].position.x = Map(Math.random(), 0, 1, camera.left, camera.right);
		particles[i].position.y = Map(Math.random(), 0, 1, camera.bottom, camera.top);
		particles[i].position.z = -1;
		velocities[i] = new THREE.Vector2(x,y);
	}*/
	 BuildFeild();

	window.addEventListener('resize', onWindowResize, false);
	Render();
});

function BuildFeild()
{
	for(var x = 0; x <= Math.floor(frustumSize * aspect); x++)
	{
		feildLines[x] = [];
		for(var y = 0; y <= Math.floor(frustumSize ); y++)
		{
			var angle = noise.simplex3(x/feildScale, y/feildScale, 0) * Math.PI * 2;
			var length = noise.simplex3(x/feildScale + 40000, y/feildScale + 40000, 0);
			length = Map(length, 0, 1, 0.1, 1);
			var g = new THREE.Geometry();
			g.vertices.push(new THREE.Vector3(0,0,0));
			g.vertices.push(new THREE.Vector3(1,0,0));
			var line = new THREE.Line(g, lineMaterial);
			scene.add(line);
			feildLines[x][y] = line;
			line.rotation.z = angle;
			line.scale.set(length, length, length);
			line.position.x = x;
			line.position.y = y;
			line.position.z = -1;
		}
	}
}

function Map(value, min, max, a, b)
{
	return Lerp(a, b, (value - min) / (max - min));
}

function Lerp(a, b, time)
{
	return a + (b - a) * time;
}

var lastTime = Date.now() * 0.01;
function Render()
{
	requestAnimationFrame(Render);
	renderer.render(scene, camera);


	var now = Date.now() * 0.01;
	// var deltaTime = now - lastTime;
	// lastTime = now;

	now *= 0.009;
	//flowFeild
	for(var x = 0; x <= Math.floor(frustumSize * aspect); x++)
	{
		feild[x] = [];
		for(var y = 0; y <= Math.floor(frustumSize); y++)
		{
			var angle = noise.simplex3(x/50, y/50, now) * Math.PI * 2;
			var length = noise.simplex3(x/50 + 40000, y/50 + 40000, now);
			length = Map(length, 0, 1, 0.1, 1);
			feild[x][y] = [angle, length];
			var line = feildLines[x][y];
			line.rotation.z = angle;
			line.scale.set(length, length, length);
		}
	}

	/*for(var i = 0; i < particles.length; i++)
	{
		var p = particles[i];
		if(p.position.x > camera.right){
			velocities[i].x *= -1;
			p.position.x = camera.left;			
		}
		else if(p.position.x < camera.left){
			velocities[i].x *= -1;
			p.position.x = camera.right;
		}
		if(p.position.y > camera.top){
			velocities[i].y *= -1;
			p.position.y = camera.bottom;			
		}
		else if(p.position.y < camera.bottom){
			velocities[i].y *= -1;
			p.position.y = camera.top;
		}

		var x = p.position.x * feildDensity;
		var y = p.position.y * feildDensity;
		x = Math.floor(Map(x, 0, feild.length, 0, feild.length-1));
		y = Math.floor(Map(y, 0, feild[0].length, 0, feild[0].length-1));
		// now *= 0.1;
		var angle = feild[x][y][0];
		var length = feild[x][y][1];

		p.position.x += (length * Math.cos(angle) ) * deltaTime * speed;
		p.position.y += (length * Math.sin(angle)) * deltaTime * speed;

	}*/

}

function onWindowResize() 
{
	aspect = window.innerWidth /  window.innerHeight;
	var w = frustumSize * aspect;
	var h = frustumSize;
	camera.aspect = aspect;
	camera.left = 0;
	camera.right = w;
	camera.top  = h;
	camera.bottom = 0;
	camera.updateProjectionMatrix();
	renderer.setSize( window.innerWidth, window.innerHeight );
	for(var x = 0; x < feildLines.length; x++)
	{
		for(var y = 0; y < feildLines[x].length; y++)
		{
			scene.remove(feildLines[x][y]);
		}
	}
	BuildFeild();
}