function Print-Help {
"
Usage: 
deploy_unity_plugins.ps1 [--with-tests]

Options:
--with-tests - build with tests.
"
}

$tests=0
$msg="Build started."
if ($args[0] -eq "--with-tests") {
    $tests=1
    $msg+=" (with tests)"
} elseif ($args[0] -eq "--help" -Or $args[0] -eq "-h") {
    Print-Help
    exit
}

$tests_info=0
Write-Host $msg -ForegroundColor Green
colcon build --merge-install --event-handlers console_direct+ --cmake-args -DCMAKE_BUILD_TYPE=Release -DBUILD_TESTING=$tests