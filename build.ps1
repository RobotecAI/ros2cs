
<#
.SYNOPSIS
    Builds 'ros2cs'
.DESCRIPTION
    This script runs colcon build
.PARAMETER with_tests
    build with tests
.PARAMETER standalone
    standalone build
.PARAMETER with_examples
    build with examples
#>
Param (
    [Parameter(Mandatory=$false)][switch]$with_tests=$false,
    [Parameter(Mandatory=$false)][switch]$standalone=$false,
    [Parameter(Mandatory=$false)][switch]$with_examples=$false
)

$msg="Build started."
$packages=,"ros2cs_core"
$tests_switch=0
if($with_tests) {
    $msg+=" (with tests)"
    $packages+="ros2cs_tests"
    $tests_switch=1
}
$standalone_switch=0
if($standalone) {
    $msg+=" (standalone)"
    $standalone_switch=1
}
if ($with_examples) {
    $msg+=" (with examples)"
    $packages+="ros2cs_examples"
}

Write-Host $msg -ForegroundColor Green
&"colcon" build `
--merge-install `
--event-handlers console_direct+ `
--cmake-args `
-DSTANDALONE_BUILD:int=$standalone_switch `
-DCMAKE_BUILD_TYPE=Release `
-DBUILD_TESTING:int=$tests_switch `
--no-warn-unused-cli `
--packages-up-to `
@packages `
@(colcon list --names-only --base-paths src/custom_packages)
