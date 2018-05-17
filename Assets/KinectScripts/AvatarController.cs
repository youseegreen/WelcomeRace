using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

//offsetNode  空のゲームオブジェクトを作ってそこにプレイヤーぶちこんでいる
//　　　　　　こいつによって、プレイヤーの位置が原点にあるとして考えられる  キャラを原点とした座標に変えているだけ
//bodyRoot　  unity世界座標上でのキャラの大元のtransform  キャラに付随して動いてる

[RequireComponent(typeof(Animator))]    //アニメーターがある時だけやるのかな？
public class AvatarController : MonoBehaviour
{	
	// Bool that has the characters (facing the player) actions become mirrored. Default false.
	public bool mirroredMovement = false;
	
	// Bool that determines whether the avatar is allowed to move in vertical direction.
	public bool verticalMovement = false;
	
	// Rate at which avatar will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
	protected float moveRate = 4.0f; //int→floatに変えた　奥行きの関数にしたほうがよさそう
	
	// Slerp smooth factor
	public float smoothFactor = 5f;
	
	// Whether the offset node must be repositioned to the user's coordinates, as reported by the sensor or not.
	public bool offsetRelativeToSensor = false;
	

	// The body root node
	protected Transform bodyRoot;
	
	// A required variable if you want to rotate the model in space.
	protected GameObject offsetNode;
	
	// Variable to hold all them bones. It will initialize the same size as initialRotations.
	protected Transform[] bones;    //ここの中身とってこやな

    public Transform[] Bones
    {
        get { return this.bones; }
    }
	
	// Rotations of the bones when the Kinect tracking starts.
	protected Quaternion[] initialRotations;
	protected Quaternion[] initialLocalRotations;
	
	// Initial position and rotation of the transform
	protected Vector3 initialPosition;
	protected Quaternion initialRotation;
	
	// Calibration Offset Variables for Character Position.
	protected bool offsetCalibrated = false;
	protected float xOffset, yOffset, zOffset;

	// private instance of the KinectManager
	protected KinectManager kinectManager;


	// transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
	private Transform _transformCache;
	public new Transform transform
	{
		get
		{
			if (!_transformCache) 
				_transformCache = base.transform;
			
			return _transformCache;
		}
	}

    //unityちゃんのボーンの位置→bones
    //  "  角度とか→initialRotations　はじめだけやから別にあるかも


    //ボーンをここでセットしている!
    public void Awake()
    {	
		// check for double start
		if(bones != null)
			return;

        //領域確保
		// inits the bones array
		bones = new Transform[22];  //22個ボーン
		// Initial rotations and directions of the bones.
		initialRotations = new Quaternion[bones.Length];
		initialLocalRotations = new Quaternion[bones.Length];
        
        //とりあえずここでunityちゃんのボーンを拾ってきている
		// Map bones to the points the Kinect tracks
		MapBones();

        //unityちゃんのボーンの位置から角度を計算している
		// Get initial bone rotations
		GetInitialRotations();
	}



    // Update the avatar each frame.
    /*全体の位置→各パーツの位置　　って流れ*/
    /*bodyRootだけ調べていけ*/
    public void UpdateAvatar(uint UserID)
    {
        if (!transform.gameObject.activeInHierarchy)
        {     //キャラがアクティブじゃないならリターン
            return;
        }
		// Get the KinectManager instance
		if(kinectManager == null)
		{
			kinectManager = KinectManager.Instance;
		}
		
        /*ここで　カメラの俺(UserID)がどう動いたかをゲットしているはず*/
        /*俺全体の位置を計算→unityちゃんの位置を計算？*/
        /*位置だけ（姿勢は動いてない）*/
		// move the avatar to its Kinect position
		MoveAvatar(UserID);

        
        /*全体の位置が決まった後→関節とかの位置を決めていっている*/
        /*unityちゃんを動かしている*/
        //ここからボーンの情報ゲットできそう
        
		for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!bones[boneIndex])  //ボーン情報がないときはパス
				continue;
			
            //この辺の内容がんばっていけ
			if(boneIndex2JointMap.ContainsKey(boneIndex))
			{
				KinectWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];

                //ボーンよ動けってやつ　なんでUserIDがいるんやろ
                TransformBone(UserID, joint, boneIndex, !mirroredMovement);
			}

            //多分ここは来ていないのかな
			else if(specIndex2JointMap.ContainsKey(boneIndex))
			{
				// special bones (clavicles)
				List<KinectWrapper.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];
				
				if(alJoints.Count >= 2)
				{
					//Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
					//TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
				}
			}
		}



        //ここにボーンと位置を入力して、技出てるか判別する機構つくる
	}
	


    /*位置は戻さんほうがいいんかな*/
    /*ユーザーがmiaしたときに呼ばれるのかな?とりあえず初期状態に戻してるっぽい*/
	// Set bones to their initial positions and rotations
	public void ResetToInitialPosition()
	{	
		if(bones == null)
			return;
		
        //全体の姿勢を戻す
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			transform.rotation = Quaternion.identity;
		}
		
        //関節の姿勢を戻す
		// For each bone that was defined, reset to initial position.
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				bones[i].rotation = initialRotations[i];
			}
		}
		
        /*ここが諸悪の根源*/
        
		if(bodyRoot != null)
		{
			bodyRoot.localPosition = Vector3.zero;
			bodyRoot.localRotation = Quaternion.identity;
		}
        
		// Restore the offset's position and rotation
		if(offsetNode != null)
		{
			offsetNode.transform.position = initialPosition;
			offsetNode.transform.rotation = initialRotation;
		}
		else
		{
			transform.position = initialPosition;
			transform.rotation = initialRotation;
		}
	}
	

    //ユーザーのキャリブレーション？  要検証
	// Invoked on the successful calibration of a player.
	public void SuccessfulCalibration(uint userId)
	{
		// reset the models position
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		
		// re-calibrate the position offset
		offsetCalibrated = false;
	}

	

    /*ここはめっちゃ大事そう*/
    /*ボーンを動かしてるけど、どこで参照しているのやら
      boneTransformがbones[index]の参照になっているんかな？　あとで調べる*/
    /*その場合ここでunityちゃんのボーンの角度(位置も？)変えてる*/
	// Apply the rotations tracked by kinect to the joints.
	protected void TransformBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {
		Transform boneTransform = bones[boneIndex]; //unityちゃんのボーンの位置とか入れる
        if (boneTransform == null || kinectManager == null)  //ボーンなかったりkinectついてなかったらパス
			return;
		
		int iJoint = (int)joint;    //関節数をいれる　　変換できんかったら－なるんかな？
		if(iJoint < 0)
			return;


        /*ここからメイン*/
        //ここでユーザーのある関節の角度ゲットしてる?
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        //取得できなかったらreturn
		if(jointRotation == Quaternion.identity)
			return;
        //ここでユーザーのさっきの情報使ってユーザーの回転角度ゲットしてる
        // Smoothly transition to the new rotation
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        //前のボーンから今のボーンに移るまでの補完方法　smoothFactorが0以外なら計算時間かかるけどスムーズに動く
		if(smoothFactor != 0f)
        	boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        //ないなら強引に新しい位置にかえる
        else
			boneTransform.rotation = newRotation;
	}


	/*特別な関節あればこっちをつかってる　　基本的な用途は上のと同じ*/
	// Apply the rotations tracked by kinect to a special joint
	protected void TransformSpecialBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, KinectWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
	{
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null || kinectManager == null)
			return;
		
		if(!kinectManager.IsJointTracked(userId, (int)joint) || 
		   !kinectManager.IsJointTracked(userId, (int)jointParent))
		{
			return;
		}
		
		Vector3 jointDir = kinectManager.GetDirectionBetweenJoints(userId, (int)jointParent, (int)joint, false, true);
		Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;
		
//		if(!flip)
//		{
//			Vector3 mirroredAngles = jointRotation.eulerAngles;
//			mirroredAngles.y = -mirroredAngles.y;
//			mirroredAngles.z = -mirroredAngles.z;
//			
//			jointRotation = Quaternion.Euler(mirroredAngles);
//		}
		
		if(jointRotation != Quaternion.identity)
		{
			// Smoothly transition to the new rotation
			Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
			
			if(smoothFactor != 0f)
				boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			else
				boneTransform.rotation = newRotation;
		}
		
	}
	


    /*ここもメイン*/
    /*kinectカメラ→人の位置→unity上でのunityちゃんの座標を計算している*/
	// Moves the avatar in 3D space - pulls the tracked position of the spine and applies it to root.
	// Only pulls positional, not rotational.
	protected void MoveAvatar(uint UserID)
	{
        if (bodyRoot == null || kinectManager == null)   //bodyのTransformの親元
			return;
        //キネクトがUserIDをトラッキングできていなければパス
        if (!kinectManager.IsJointTracked(UserID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter))
			return;

        /*transにUserIDの人の座標をいれている*/
        // Get the position of the body and store it.
        Vector3 trans = kinectManager.GetUserPosition(UserID);


        /*一回目だけキャリブレーションしてる*/
        /*xyzOffsetでunity上での初期座標をゲットしている*/
        /*多分kinectの座標が数学の座標と同じで（画面左ならー）moveRateは実空間とunity空間の倍率になってる*/
        // If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
        if (!offsetCalibrated)
		{
			offsetCalibrated = true;
			
			xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
			yOffset = trans.y * moveRate;
			zOffset = -trans.z * moveRate;

            /*よくわからない　要検証*/  /*はいってないっぽい*/
                             /*kinectカメラと人の位置関係をゲットしてるんかな？*/
            if (offsetRelativeToSensor)
            {
                //kinectカメラのポジション
                Vector3 cameraPos = Camera.main.transform.position;

                float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
                Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
                Vector3 offsetPos = cameraPos + relativePos;

                if (offsetNode != null)
                {
                    offsetNode.transform.position = offsetPos;
                }
                else
                {
                    transform.position = offsetPos;
                }
            }
        }
	
        //ここがメイン部分な気がする  何をしているのだろうか
        //trans：人の位置　
		// Smoothly transition to the new position
		Vector3 targetPos = Kinect2AvatarPos(trans, verticalMovement);

        //人の位置をだいたいここで動かす　smoothFactorあれば補正しながら動かす
        if (smoothFactor != 0f) {
            bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
        }
        else
            bodyRoot.localPosition = targetPos;

        //bodyRootがunity上での位置なのか　kinect上での位置なのか　調べていけ  多分unity上かな？
        /*trans：kinect空間　　targetPos bodyRoot：unity空間　　と思っている　あってる？*/
	}
	

    //unityちゃんのボーンをここでいれているっぽい
	// If the bones to be mapped have been declared, map that bone to the model.
	protected virtual void MapBones()
	{
		// make OffsetNode as a parent of model transform.
		offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
		offsetNode.transform.position = transform.position;
		offsetNode.transform.rotation = transform.rotation;
		offsetNode.transform.parent = transform.parent;
		
		transform.parent = offsetNode.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		
		// take model transform as body root
		bodyRoot = transform;   //bodyRootはunity世界座標
        
        //キャラのボーンをとる
        // get bone transforms from the animator component
        var animatorComponent = GetComponent<Animator>();       //animatorComponenでunityちゃんの情報パクってくる

        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!boneIndex2MecanimMap.ContainsKey(boneIndex)) 
				continue;
            //アニメータの中にGetBoneTransformがあってそいつをボーンに入れている
			bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]);
		}
	}
	

    /*ボーンの位置から角度を計算する  初期用*/
	// Capture the initial rotations of the bones
	protected void GetInitialRotations()
	{
		// save the initial rotation
		if(offsetNode != null)
		{
			initialPosition = offsetNode.transform.position;
			initialRotation = offsetNode.transform.rotation;
			
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			initialPosition = transform.position;
			initialRotation = transform.rotation;
			
			transform.rotation = Quaternion.identity;
		}
		
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation; // * Quaternion.Inverse(initialRotation);
				initialLocalRotations[i] = bones[i].localRotation;
			}
		}
		
		// Restore the initial rotation
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		else
		{
			transform.rotation = initialRotation;
		}
	}
	





	// Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
	protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
	{
		// Apply the new rotation.
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		
		//If an offset node is specified, combine the transform with its
		//orientation to essentially make the skeleton relative to the node
		if (offsetNode != null)
		{
			// Grab the total rotation by adding the Euler and offset's Euler.
			Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
			// Grab our new rotation.
			newRotation = Quaternion.Euler(totalRotation);
		}
		
		return newRotation;
	}
	



    /*Posがunity上か　kinect上かはっきりしていけ*/
    /*こいつが　人の移動のメイン部分*/
	// Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
	protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
	{
        //jointPosition：キネクト画像上？での人の位置

        //今unity上?で　人はどこにいるのでしょうか　を決めている?
        float xPos;
		float yPos;
		float zPos;

        //moveRateかけているからunity空間上？
        // If movement is mirrored, reverse it.
        if (!mirroredMovement)
            xPos = jointPosition.x * moveRate;// - xOffset;
        else 
        	xPos = -jointPosition.x * moveRate;// - xOffset;		
        yPos = jointPosition.y * moveRate - yOffset;
        zPos = -jointPosition.z * moveRate - zOffset;



        //人?の関節の位置→
        // If we are tracking vertical movement, update the y. Otherwise leave it alone.
        //     Vector3 avatarJointPos = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);
        if (yPos < 0) yPos = 0;
        Vector3 avatarJointPos = new Vector3(xPos, yPos,0);
   //     Vector3 avatarJointPos = new Vector3(xPos, 0, 0);
        return avatarJointPos;
	}
	




    /*ここから下でキャラのボーンの位置はかっているん?*/

	// dictionaries to speed up bones' processing
	// the author of the terrific idea for kinect-joints to mecanim-bones mapping
	// along with its initial implementation, including following dictionary is
	// Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
	private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
		{2, HumanBodyBones.Neck},
		{3, HumanBodyBones.Head},
		
		{4, HumanBodyBones.LeftShoulder},
		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
		{8, HumanBodyBones.LeftIndexProximal},

		{9, HumanBodyBones.RightShoulder},
		{10, HumanBodyBones.RightUpperArm},
		{11, HumanBodyBones.RightLowerArm},
		{12, HumanBodyBones.RightHand},
		{13, HumanBodyBones.RightIndexProximal},

		{14, HumanBodyBones.LeftUpperLeg},
		{15, HumanBodyBones.LeftLowerLeg},
		{16, HumanBodyBones.LeftFoot},
		{17, HumanBodyBones.LeftToes},
		
		{18, HumanBodyBones.RightUpperLeg},
		{19, HumanBodyBones.RightLowerLeg},
		{20, HumanBodyBones.RightFoot},
		{21, HumanBodyBones.RightToes},
	};
	
    /*ここみたほうがよいかも*/
	protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2JointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{6, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{7, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
		{8, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{11, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{12, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
		{13, KinectWrapper.NuiSkeletonPositionIndex.HandRight},

        {14, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        {15, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        {16, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
        {17, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},

        {18, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        {19, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        {20, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
        {21, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
    };
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
	protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2MirrorJointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{6, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{7, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
		{8, KinectWrapper.NuiSkeletonPositionIndex.HandRight},
		
		{10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{11, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{12, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
		{13, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{14, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
		{15, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
		{16, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
		{17, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
		
		{18, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
		{19, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
		{20, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
		{21, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},
	};
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
}

