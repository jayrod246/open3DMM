const std = @import("std");

pub fn build(b: *std.Build) void {
    // No other target is supported right now
    const target = b.resolveTargetQuery(.{
        .cpu_arch = .x86,
        .os_tag = .windows,
        .abi = .gnu,
    });

    const optimize = b.standardOptimizeOption(.{});

    const @"open3dmm-core_dep" = b.dependency("open3dmm-core", .{
        .target = target,
        .optimize = optimize,
    });

    const exe = b.addExecutable(.{
        .name = "open3DMM",
        .target = target,
        .optimize = optimize,
        .link_libc = true,
    });
    exe.root_module.sanitize_c = false;

    exe.linkLibrary(@"open3dmm-core_dep".artifact("open3dmm-core"));
    exe.installLibraryHeaders(@"open3dmm-core_dep".artifact("open3dmm-core"));
    exe.addIncludePath(.{ .path = "inc" });
    exe.addIncludePath(.{ .path = "src/studio" });
    exe.addCSourceFiles(.{ .files = open3dmm_sources, .flags = open3dmm_flags });

    exe.addWin32ResourceFile(.{
        .file = .{ .path = "src/studio/utest.rc" },
        .flags = &.{
            "/y",
            "/i",
            b.pathFromRoot("inc"),
            "/i",
            b.getInstallPath(.header, ""),
            "/i",
            b.pathFromRoot("src"),
        },
    });

    b.installArtifact(exe);

    const run_cmd = b.addRunArtifact(exe);
    run_cmd.step.dependOn(b.getInstallStep());

    // By making the run step depend on the install step, it will be run from the
    // installation directory rather than directly from within the cache directory.
    // This is not necessary, however, if the application depends on other installed
    // files, this ensures they will be present and in the expected location.
    run_cmd.step.dependOn(b.getInstallStep());

    if (b.args) |args| {
        run_cmd.addArgs(args);
    }

    const run_step = b.step("run", "Run the app");
    run_step.dependOn(&run_cmd.step);
}

const open3dmm_flags: []const []const u8 = &.{
    "-w",
    "-DLITTLE_ENDIAN",
    "-DWIN",
    "-DSTRICT",
    "-fms-extensions",
    "-fno-rtti",
    "-fno-exceptions",
};

const open3dmm_sources: []const []const u8 = &.{
    "src/studio/ape.cpp",
    "src/studio/browser.cpp",
    "src/studio/esl.cpp",
    "src/studio/mminstal.cpp",
    "src/studio/popup.cpp",
    "src/studio/portf.cpp",
    "src/studio/scnsort.cpp",
    "src/studio/splot.cpp",
    "src/studio/stdiobrw.cpp",
    "src/studio/stdioscb.cpp",
    "src/studio/studio.cpp",
    "src/studio/tatr.cpp",
    "src/studio/tgob.cpp",
    "src/studio/utest.cpp",
};
