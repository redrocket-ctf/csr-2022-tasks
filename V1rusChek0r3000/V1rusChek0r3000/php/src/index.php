<html><head><title>Virus Check0r</title>
<body>
<div class="box">
<h1>V1rusCheckor3000</h1>

<?php

if (isset($_FILES["file"])) {
    $size = $_FILES["file"]['size'];

    if ($size > 100) {
        echo "For such large files, buy premium";
    } else {
	    $target_dir = "uploads/";
	    $target_file = $target_dir . $_FILES["file"]["name"];
	    
	    move_uploaded_file($_FILES["file"]["tmp_name"], $target_file);
	    
	    function hasVirus($file_path) {
	    	# Check for Virus
	    	$argument = escapeshellarg($file_path);
	    	exec("clamscan $argument", $output, $retval);
	    
	    	if ($retval != 0) {
	    		return true;
	    	} 
	    	return false;
	    }
	    
	    if (hasVirus($target_file)) {
	    	echo "The file contains a virus!";
	    } else {
	    	echo "The file is safe to use!";
	    }
	    
        unlink($target_file);
    }
	
}

if (isset($_GET["source"])) {
	highlight_file(__FILE__);
}
?>

<form action="index.php" method="post" enctype="multipart/form-data">
  Select suspicious file to upload:
  <input type="file" name="file">
  <input type="submit" value="Upload" name="submit">
</form>



</div> 

</form>
This app is <a href="/?source=1">open source</a>.
</body>
</html>
